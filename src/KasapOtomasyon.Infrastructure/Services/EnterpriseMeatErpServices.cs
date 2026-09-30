using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Services;

public class MassBalanceAndCuttingService : IMassBalanceAndCuttingService
{
    private readonly KasapDbContext _context;
    private readonly IAuditService _auditService;

    public MassBalanceAndCuttingService(KasapDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<CuttingOrder> CreateCuttingOrderAsync(string carcassNumber, decimal inputWeightKg, string masterButcher)
    {
        var slaughter = await _context.SlaughterRecords.Include(s => s.AnimalIntake).FirstOrDefaultAsync(s => s.CarcassNumber == carcassNumber);
        var order = new CuttingOrder
        {
            OrderCode = $"CUT-{DateTime.UtcNow:yyyyMMdd}-{Math.Abs(carcassNumber.GetHashCode()) % 10000:D4}",
            SlaughterRecordId = slaughter?.Id ?? 0,
            CarcassNumber = carcassNumber,
            EarTagNumber = slaughter?.AnimalIntake?.EarTagNumber ?? "",
            InputCarcassWeightKg = inputWeightKg,
            MasterButcher = masterButcher,
            StartTime = DateTime.UtcNow,
            Status = CuttingOrderStatus.InProgress
        };

        await _context.CuttingOrders.AddAsync(order);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CUTTING_ORDER_CREATED", "CuttingOrder", order.OrderCode, $"Giriş Karkas: {inputWeightKg:N2} kg | Kasap: {masterButcher}");
        return order;
    }

    public async Task<MassBalanceCalculationResult> ValidateMassBalanceAsync(int cuttingOrderId)
    {
        var order = await _context.CuttingOrders.Include(o => o.Cuts).FirstOrDefaultAsync(o => o.Id == cuttingOrderId);
        if (order == null) throw new InvalidOperationException("Parçalama emri bulunamadı.");

        var input = order.InputCarcassWeightKg;
        var output = order.TotalOutputWeightKg;
        var diff = order.MassBalanceDifferenceKg;
        var varPercent = order.MassBalanceVariancePercentage;
        var isApproved = order.IsMassBalanceApproved;

        var message = isApproved 
            ? $"✓ Kütle Dengesi Doğrulandı: Giriş: {input:N2} kg, Çıkış: {output:N2} kg (Varyans: %{varPercent:N2} <= %0.50)"
            : $"⚠️ DİKKAT: Kütle Dengesi Sınır Dışı! Giriş: {input:N2} kg, Çıkış: {output:N2} kg (Açıklanamayan Fark: {diff:N2} kg, Varyans: %{varPercent:N2})";

        return new MassBalanceCalculationResult
        {
            OrderCode = order.OrderCode,
            InputWeightKg = input,
            OutputWeightKg = output,
            DifferenceKg = diff,
            VariancePercentage = varPercent,
            IsApproved = isApproved,
            Message = message
        };
    }

    public async Task<CuttingOrder> FinalizeCuttingOrderAsync(int cuttingOrderId, List<CarcassDeboningCut> cuts, decimal boneKg, decimal fatKg, decimal wasteKg, decimal processLossKg)
    {
        var order = await _context.CuttingOrders.FirstOrDefaultAsync(o => o.Id == cuttingOrderId);
        if (order == null) throw new InvalidOperationException("Parçalama emri bulunamadı.");

        order.TotalPrimalCutsWeightKg = cuts.Sum(c => c.WeightKg);
        order.TotalBoneAndFatWeightKg = boneKg + fatKg;
        order.TotalWasteWeightKg = wasteKg;
        order.ProcessLossWeightKg = processLossKg;
        order.EndTime = DateTime.UtcNow;

        if (order.IsMassBalanceApproved)
        {
            order.Status = CuttingOrderStatus.MassBalanceVerified;
        }
        else
        {
            order.Status = CuttingOrderStatus.MassBalanceVarianceWarning;
            order.VarianceExplanation = $"Kütle dengesinde {order.MassBalanceDifferenceKg:N2} kg açıklanamayan sapma kaydedildi.";
        }

        // Attach cuts
        foreach (var cut in cuts)
        {
            cut.SlaughterRecordId = order.SlaughterRecordId;
            cut.CarcassNumber = order.CarcassNumber;
            cut.EarTagNumber = order.EarTagNumber;
            cut.CutDate = DateTime.UtcNow;
            cut.MasterButcher = order.MasterButcher;
        }

        await _context.CarcassDeboningCuts.AddRangeAsync(cuts);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            "CUTTING_ORDER_FINALIZED", 
            "CuttingOrder", 
            order.OrderCode, 
            $"Kütle Dengesi: Giriş: {order.InputCarcassWeightKg:N2} kg, Çıkış: {order.TotalOutputWeightKg:N2} kg, Varyans: %{order.MassBalanceVariancePercentage:N2}",
            reason: order.VarianceExplanation);

        return order;
    }

    public async Task<List<CuttingOrder>> GetCuttingOrdersAsync(int plantId = 1)
    {
        return await _context.CuttingOrders
            .Where(o => o.PlantId == plantId)
            .OrderByDescending(o => o.StartTime)
            .ToListAsync();
    }
}

public class FoodRecallAndHaccpService : IFoodRecallAndHaccpService
{
    private readonly KasapDbContext _context;
    private readonly IAuditService _auditService;

    public FoodRecallAndHaccpService(KasapDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<FoodRecallAnalysisResult> Trigger1ClickRecallAsync(string rootCauseEarTagOrCarcass, string triggerReason)
    {
        var cleanQuery = rootCauseEarTagOrCarcass.Trim();

        // 1. Trace all cuts and lots produced from this animal
        var cuts = await _context.CarcassDeboningCuts
            .Where(c => c.EarTagNumber.ToLower() == cleanQuery.ToLower() || c.CarcassNumber.ToLower() == cleanQuery.ToLower())
            .ToListAsync();

        var affectedLots = cuts.Select(c => c.LotNumber).Distinct().ToList();
        var totalWeight = cuts.Sum(c => c.WeightKg);
        var totalVal = cuts.Sum(c => c.TotalCutValue);

        var caseNumber = $"RCL-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        var recallCase = new FoodRecallCase
        {
            CaseNumber = caseNumber,
            Title = $"Acil Geri Çağırma & Karantina Vakası: {cleanQuery}",
            TriggerReason = triggerReason,
            RiskLevel = RecallRiskLevel.CriticalClass1,
            Status = RecallStatus.ActiveQuarantine,
            RootCauseEarTag = cleanQuery,
            AffectedLotCount = affectedLots.Count,
            AffectedCustomerCount = 3, // B2B Customers
            TotalRecalledWeightKg = totalWeight,
            EstimatedFinancialLoss = totalVal,
            InitiatedByUserName = "Kalite Güvence & HACCP Müdürü"
        };

        await _context.FoodRecallCases.AddAsync(recallCase);
        await _context.SaveChangesAsync();

        var recallItems = new List<FoodRecallItem>();
        foreach (var cut in cuts)
        {
            var item = new FoodRecallItem
            {
                FoodRecallCaseId = recallCase.Id,
                LotNumber = cut.LotNumber,
                ProductName = cut.CutName,
                QuantityKg = cut.WeightKg,
                WarehouseLocation = cut.TargetStorageLocation,
                CustomerName = "Toptan Et Müşterisi #A (Sevkiyatta Bloke)",
                InvoiceNumber = "FAT-2026-00892",
                ItemState = RecallItemState.QuarantinedInWarehouse,
                ResolutionNotes = "Satış ve sevkiyat terminal seviyesinde derhal durduruldu."
            };
            recallItems.Add(item);
        }

        await _context.FoodRecallItems.AddRangeAsync(recallItems);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            "FOOD_RECALL_TRIGGERED", 
            "FoodRecallCase", 
            caseNumber, 
            $"KRİTİK GERİ ÇAĞIRMA: {cleanQuery} | Neden: {triggerReason} | Etkilenen Lot Sayısı: {affectedLots.Count} | Toplam Kg: {totalWeight:N2}",
            reason: triggerReason,
            supervisorApprover: "Kalite Güvence Müdürü");

        return new FoodRecallAnalysisResult
        {
            CaseNumber = caseNumber,
            RootCause = cleanQuery,
            AffectedLotsCount = affectedLots.Count,
            AffectedCustomersCount = 3,
            TotalRecalledWeightKg = totalWeight,
            EstimatedFinancialLoss = totalVal,
            AffectedItems = recallItems
        };
    }

    public async Task<HaccpInspectionRecord> RecordHaccpMeasurementAsync(int ccpId, string targetIdentifier, decimal measuredValue, string inspectorName)
    {
        var ccp = await _context.HaccpControlPoints.FirstOrDefaultAsync(c => c.Id == ccpId);
        bool isViolation = false;
        string? notes = null;

        if (ccp != null)
        {
            if (measuredValue < ccp.CriticalMinLimit || measuredValue > ccp.CriticalMaxLimit)
            {
                isViolation = true;
                notes = $"HACCP LİMİT İHLALİ! Ölçülen: {measuredValue} {ccp.UnitOfLimit} (İzin Verilen Aralık: {ccp.CriticalMinLimit} - {ccp.CriticalMaxLimit} {ccp.UnitOfLimit}). {ccp.CorrectiveActionProcedure}";
            }
        }

        var record = new HaccpInspectionRecord
        {
            HaccpControlPointId = ccpId,
            TargetIdentifier = targetIdentifier,
            MeasuredValue = measuredValue,
            IsViolation = isViolation,
            AutoHoldTriggered = isViolation,
            InspectorName = inspectorName,
            CorrectiveActionNotes = notes,
            InspectionTimestamp = DateTime.UtcNow
        };

        await _context.HaccpInspectionRecords.AddAsync(record);
        await _context.SaveChangesAsync();

        if (isViolation)
        {
            await _auditService.LogAsync(
                "HACCP_CCP_VIOLATION", 
                "HaccpControlPoint", 
                targetIdentifier, 
                notes ?? "Kritik limit aşıldı.", 
                reason: notes);
        }

        return record;
    }

    public async Task<List<FoodRecallCase>> GetActiveRecallCasesAsync()
    {
        return await _context.FoodRecallCases
            .Include(r => r.AffectedItems)
            .OrderByDescending(r => r.InitiatedDate)
            .ToListAsync();
    }

    public async Task<bool> ResolveRecallCaseAsync(int caseId, string resolutionNotes)
    {
        var recallCase = await _context.FoodRecallCases.FirstOrDefaultAsync(r => r.Id == caseId);
        if (recallCase == null) return false;

        recallCase.Status = RecallStatus.ResolvedAndClosed;
        recallCase.ClosedDate = DateTime.UtcNow;
        recallCase.ClosureResolution = resolutionNotes;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("FOOD_RECALL_RESOLVED", "FoodRecallCase", recallCase.CaseNumber, resolutionNotes);
        return true;
    }
}

public class TrueCostEngineService : ITrueCostEngineService
{
    private readonly KasapDbContext _context;

    public TrueCostEngineService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<TrueCostRollup> CalculateTrueCostAsync(string carcassNumber)
    {
        var slaughter = await _context.SlaughterRecords
            .Include(s => s.AnimalIntake)
            .Include(s => s.DeboningCuts)
            .FirstOrDefaultAsync(s => s.CarcassNumber == carcassNumber);

        if (slaughter == null) throw new InvalidOperationException($"'{carcassNumber}' karkas kaydı bulunamadı.");

        var coldKg = slaughter.ColdCarcassWeightKg > 0 ? slaughter.ColdCarcassWeightKg : slaughter.HotCarcassWeightKg;
        var hideCredit = slaughter.HideWeightKg * 25.0m;   // 25 TL/kg deri geliri
        var offalCredit = slaughter.OffalWeightKg * 60.0m; // 60 TL/kg sakatat geliri
        var deboningLabor = 1500.0m;
        var packagingCost = 800.0m;

        var cutsTotalValue = slaughter.DeboningCuts.Sum(c => c.TotalCutValue);

        var rollup = new TrueCostRollup
        {
            TenantId = slaughter.AnimalIntake?.TenantId ?? 1,
            CompanyId = slaughter.AnimalIntake?.CompanyId ?? 1,
            PlantId = slaughter.AnimalIntake?.PlantId ?? 1,
            CarcassNumber = carcassNumber,
            EarTagNumber = slaughter.AnimalIntake?.EarTagNumber ?? "",
            AnimalPurchaseCost = slaughter.AnimalPurchaseCost > 0 ? slaughter.AnimalPurchaseCost : (slaughter.AnimalIntake?.PurchasePrice ?? 0m),
            LivestockTransportCost = slaughter.TransportationCost > 0 ? slaughter.TransportationCost : 3000.0m,
            SlaughterLaborCost = slaughter.SlaughterLaborCost > 0 ? slaughter.SlaughterLaborCost : 2500.0m,
            VeterinaryInspectionCost = 750.0m,
            ColdStorageChillingEnergyCost = slaughter.CoolingElectricityCost > 0 ? slaughter.CoolingElectricityCost : 1500.0m,
            DeboningLaborCost = deboningLabor,
            PackagingAndLabelingCost = packagingCost,
            GeneralFacilityOverheadCost = slaughter.GeneralOverheadCost > 0 ? slaughter.GeneralOverheadCost : 2000.0m,
            HideLeatherCreditRevenue = hideCredit,
            OffalOrganCreditRevenue = offalCredit,
            BoneAndFatCreditRevenue = 500.0m,
            ColdCarcassWeightKg = coldKg,
            TotalDebonedMeatSalesValue = cutsTotalValue
        };

        return rollup;
    }

    public async Task<List<TrueCostRollup>> GetPlantCostRollupsAsync(int plantId = 1)
    {
        var slaughters = await _context.SlaughterRecords
            .Include(s => s.AnimalIntake)
            .Include(s => s.DeboningCuts)
            .OrderByDescending(s => s.SlaughterDate)
            .Take(50)
            .ToListAsync();

        var list = new List<TrueCostRollup>();
        foreach (var s in slaughters)
        {
            list.Add(await CalculateTrueCostAsync(s.CarcassNumber));
        }

        return list;
    }
}

public class AntiFraudEngineService : IAntiFraudEngineService
{
    private readonly KasapDbContext _context;
    private readonly IAuditService _auditService;

    public AntiFraudEngineService(KasapDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public FraudRiskEvaluationResult EvaluateScaleOverride(decimal oldWeightKg, decimal newWeightKg, string operatorName)
    {
        var diff = Math.Abs(newWeightKg - oldWeightKg);
        var diffRatio = oldWeightKg > 0 ? (diff / oldWeightKg) * 100m : 100m;

        if (diffRatio > 10.0m || diff > 20.0m)
        {
            return new FraudRiskEvaluationResult
            {
                RiskLevel = FraudRiskLevel.Critical,
                RiskScore = 95.0m,
                RequiresSupervisorApproval = true,
                WarningMessage = $"🚨 KRİTİK RİSK: Kantar ağırlığında {diff:N2} kg (%{diffRatio:N1}) manuel değişiklik tespit edildi. Tesis Müdürü onayı zorunludur."
            };
        }

        if (diffRatio > 3.0m)
        {
            return new FraudRiskEvaluationResult
            {
                RiskLevel = FraudRiskLevel.High,
                RiskScore = 70.0m,
                RequiresSupervisorApproval = true,
                WarningMessage = $"⚠️ YÜKSEK RİSK: Kantar ağırlığında %{diffRatio:N1} manuel sapma. Süpervizör onayı gerekli."
            };
        }

        return new FraudRiskEvaluationResult
        {
            RiskLevel = FraudRiskLevel.Low,
            RiskScore = 15.0m,
            RequiresSupervisorApproval = false,
            WarningMessage = "Normal tolerans dahilinde kantar düzeltmesi."
        };
    }

    public FraudRiskEvaluationResult EvaluateCuttingVariance(decimal inputKg, decimal outputKg)
    {
        var diff = Math.Abs(inputKg - outputKg);
        var ratio = inputKg > 0 ? (diff / inputKg) * 100m : 0m;

        if (ratio > 2.0m)
        {
            return new FraudRiskEvaluationResult
            {
                RiskLevel = FraudRiskLevel.Critical,
                RiskScore = 90.0m,
                RequiresSupervisorApproval = true,
                WarningMessage = $"🚨 ANORMAL PARÇALAMA FİRESİ: %{ratio:N2} ({diff:N2} kg) kayıp. Çalınma veya kayıt dışı çıkış şüphesi."
            };
        }

        return new FraudRiskEvaluationResult
        {
            RiskLevel = FraudRiskLevel.Low,
            RiskScore = 10.0m,
            RequiresSupervisorApproval = false,
            WarningMessage = "Kütle dengesi kabul edilebilir standartlar içinde."
        };
    }

    public FraudRiskEvaluationResult EvaluateSaleBelowCost(decimal unitCost, decimal unitPrice)
    {
        if (unitPrice < unitCost)
        {
            var lossPerKg = unitCost - unitPrice;
            return new FraudRiskEvaluationResult
            {
                RiskLevel = FraudRiskLevel.High,
                RiskScore = 80.0m,
                RequiresSupervisorApproval = true,
                WarningMessage = $"⚠️ MALİYET ALTI SATIŞ: Birim Maliyet: {unitCost:C2}, Satış Fiyatı: {unitPrice:C2} (Kg Başı Zarar: {lossPerKg:C2})"
            };
        }

        return new FraudRiskEvaluationResult
        {
            RiskLevel = FraudRiskLevel.Low,
            RiskScore = 5.0m,
            RequiresSupervisorApproval = false,
            WarningMessage = "Satış fiyatı maliyetin üzerinde, kârlı işlem."
        };
    }

    public async Task<FraudInvestigationCase> CreateFraudCaseAsync(string action, string suspectedUser, decimal lossAmount, string evidence)
    {
        var caseCode = $"FRD-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        var fraudCase = new FraudInvestigationCase
        {
            CaseCode = caseCode,
            Title = $"Şüpheli İşlem Soruşturması: {action}",
            TriggerAction = action,
            SuspectedUserName = suspectedUser,
            EstimatedFinancialImpact = lossAmount,
            EvidencePayload = evidence,
            RiskLevel = FraudRiskLevel.High,
            Status = FraudCaseStatus.OpenUnderReview
        };

        await _context.FraudInvestigationCases.AddAsync(fraudCase);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            "FRAUD_CASE_OPENED", 
            "FraudInvestigationCase", 
            caseCode, 
            $"Soruşturma Başlatıldı: {action} | Kullanıcı: {suspectedUser} | Kayıp: {lossAmount:C2}",
            reason: evidence);

        return fraudCase;
    }

    public async Task<List<FraudInvestigationCase>> GetOpenFraudCasesAsync()
    {
        return await _context.FraudInvestigationCases
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}

// =========================================================================
// 5. MEAT PROCESSING RECIPES & BATCH PRODUCTION (PHASE 8)
// =========================================================================
public class MeatProcessingService : IMeatProcessingService
{
    private readonly KasapDbContext _context;
    private readonly IAuditService _auditService;

    public MeatProcessingService(KasapDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<ProcessingRecipe>> GetRecipesAsync(int plantId = 1)
    {
        return await _context.ProcessingRecipes
            .Include(r => r.RecipeItems)
            .Where(r => r.IsActive)
            .OrderBy(r => r.RecipeName)
            .ToListAsync();
    }

    public async Task<ProcessingRecipe> CreateRecipeAsync(ProcessingRecipe recipe, List<ProcessingRecipeItem> items)
    {
        await _context.ProcessingRecipes.AddAsync(recipe);
        await _context.SaveChangesAsync();

        foreach (var item in items)
        {
            item.ProcessingRecipeId = recipe.Id;
            await _context.ProcessingRecipeItems.AddAsync(item);
        }

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("CREATE_PROCESSING_RECIPE", "ProcessingRecipe", recipe.RecipeCode, $"Reçete Tanımlandı: {recipe.RecipeName}");
        return recipe;
    }

    public async Task<ProductionOrder> ExecuteProcessingBatchAsync(int recipeId, decimal targetBatchWeightKg, string operatorName)
    {
        var recipe = await _context.ProcessingRecipes
            .Include(r => r.RecipeItems)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null)
            throw new InvalidOperationException($"Reçete bulunamadı (ID: {recipeId})");

        var orderNumber = $"PRD-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        var outputWeight = targetBatchWeightKg * (recipe.ExpectedYieldPercentage / 100.0m);
        var wasteWeight = targetBatchWeightKg - outputWeight;

        var prodOrder = new ProductionOrder
        {
            OrderNumber = orderNumber,
            ResponsiblePerson = operatorName,
            OrderDate = DateTime.UtcNow,
            InputWeightKg = targetBatchWeightKg,
            TotalOutputWeightKg = outputWeight,
            TotalWasteWeightKg = wasteWeight,
            Status = ProductionOrderStatus.Tamamlandi,
            Notes = $"Reçete: {recipe.RecipeCode} - {recipe.RecipeName}"
        };

        await _context.ProductionOrders.AddAsync(prodOrder);
        await _context.SaveChangesAsync();

        decimal totalInputCost = 0m;

        // Add Inputs based on recipe percentage
        foreach (var item in recipe.RecipeItems)
        {
            var allocatedKg = targetBatchWeightKg * (item.PercentageRatio / 100.0m);
            totalInputCost += allocatedKg * item.StandardUnitCost;

            var input = new ProductionInput
            {
                ProductionOrderId = prodOrder.Id,
                ItemName = $"{item.ItemName} ({item.PercentageRatio:N1}%)",
                WeightKg = allocatedKg,
                UnitCost = item.StandardUnitCost
            };
            await _context.ProductionInputs.AddAsync(input);
        }

        prodOrder.InputCostTotal = totalInputCost;
        prodOrder.TotalOutputCostTotal = totalInputCost;

        // Add Finished Product Output
        var output = new ProductionOutput
        {
            ProductionOrderId = prodOrder.Id,
            CutName = recipe.RecipeName,
            WeightKg = outputWeight,
            CostPerKg = outputWeight > 0 ? Math.Round(totalInputCost / outputWeight, 2) : 0m,
            YieldPercentage = recipe.ExpectedYieldPercentage,
            BatchBarcode = $"2900{prodOrder.Id:D4}{Convert.ToInt32(outputWeight * 100):D5}",
            ExpiryDate = DateTime.UtcNow.AddDays(14),
            IsWaste = false,
            IsByproduct = false
        };
        await _context.ProductionOutputs.AddAsync(output);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("EXECUTE_BATCH_PRODUCTION", "ProductionOrder", orderNumber, $"Parti Üretimi Tamamlandı: {recipe.RecipeName} ({targetBatchWeightKg:N1} kg hammadde -> {outputWeight:N1} kg mamul)");
        return prodOrder;
    }
}

// =========================================================================
// 6. WMS & SSCC-18 PALLET LOGISTICS SUBSYSTEM (PHASE 9 & 13)
// =========================================================================
public class WmsPalletLogisticsService : IWmsPalletLogisticsService
{
    private readonly KasapDbContext _context;
    private readonly IAuditService _auditService;

    public WmsPalletLogisticsService(KasapDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<PalletSSCC> BuildPalletSSCCAsync(string productName, string lotNumber, int boxCount, decimal netWeightKg, string warehouseBin, string customer = "")
    {
        var serialNumber = new Random().Next(100000000, 999999999).ToString();
        var sscc18 = GenerateSscc18("0", "8690001", serialNumber);
        var palletNumber = $"PAL-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var pallet = new PalletSSCC
        {
            PalletNumber = palletNumber,
            Sscc18Barcode = sscc18,
            LotNumber = lotNumber,
            ProductName = productName,
            TotalBoxesCount = boxCount,
            TotalNetWeightKg = netWeightKg,
            TotalGrossWeightKg = netWeightKg + (boxCount * 0.8m) + 25.0m, // Palet darası + koli ağırlığı
            TargetWarehouseBin = warehouseBin,
            DestinationCustomer = customer,
            IsAllocatedForShipment = !string.IsNullOrWhiteSpace(customer),
            IsQuarantined = false
        };

        await _context.PalletSSCCs.AddAsync(pallet);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("BUILD_SSCC_PALLET", "PalletSSCC", sscc18, $"SSCC-18 Palet Oluşturuldu: {palletNumber} ({boxCount} Koli, {netWeightKg:N2} kg) -> Raf: {warehouseBin}");
        return pallet;
    }

    public async Task<List<PalletSSCC>> GetActivePalletsAsync(int plantId = 1)
    {
        return await _context.PalletSSCCs
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> QuarantinePalletAsync(int palletId, string reason)
    {
        var pallet = await _context.PalletSSCCs.FirstOrDefaultAsync(p => p.Id == palletId);
        if (pallet == null) return false;

        pallet.IsQuarantined = true;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("QUARANTINE_PALLET", "PalletSSCC", pallet.Sscc18Barcode, $"Palet Karantinaya Alındı: {pallet.PalletNumber}. Gerekçe: {reason}");
        return true;
    }

    public string GenerateSscc18(string extensionDigit, string gcp, string serialNumber)
    {
        // SSCC-18 Structure: Extension Digit (1) + GS1 Company Prefix (7-10) + Serial Reference + Check Digit (1)
        var raw17 = $"{extensionDigit}{gcp}{serialNumber}".PadRight(17, '0').Substring(0, 17);
        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            int digit = raw17[i] - '0';
            sum += (i % 2 == 0) ? digit * 3 : digit * 1;
        }

        int remainder = sum % 10;
        int checkDigit = (remainder == 0) ? 0 : 10 - remainder;
        return raw17 + checkDigit;
    }
}
