using FluentAssertions;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Services;
using Xunit;

namespace KasapOtomasyon.Tests;

public class ShelfLifeReprocessingTests
{
    [Fact]
    public void ExpiringBatch_RemainingDaysCalculation_ShouldFlagCriticalBatches()
    {
        var stockItem = new StockItem
        {
            ProductId = 4,
            CurrentQuantity = 25.5m,
            LotNumber = "LOT-2026-KYM-01",
            ExpiryDate = DateTime.UtcNow.AddDays(5),
            ShelfDisplayExpiryDate = DateTime.UtcNow.AddDays(1) // 1 day left on retail shelf!
        };

        stockItem.ReyonExpiryDate.Should().Be(stockItem.ShelfDisplayExpiryDate.Value);
        stockItem.ReyonDaysRemaining.Should().BeLessThanOrEqualTo(1.1);
        stockItem.IsReyonCritical.Should().BeTrue();
    }

    [Fact]
    public void ReprocessingSimulation_SausageBatch_ShouldScaleIngredientsAndDryingYield()
    {
        // 70% meat, 20% fat, 5% spices, 2% salt, 3% casing = 100%
        // Expected yield 90% (10% water evaporation during fermentation)
        decimal rawMeatKg = 70.0m;
        decimal meatRatio = 70.0m;
        decimal totalBatchKg = rawMeatKg / (meatRatio / 100.0m); // 100 kg
        decimal expectedYieldPct = 90.0m;
        decimal finishedProductKg = totalBatchKg * (expectedYieldPct / 100.0m); // 90 kg

        totalBatchKg.Should().Be(100.0m);
        finishedProductKg.Should().Be(90.0m);

        // Required spices = 100 * 0.05 = 5.0 kg
        decimal requiredSpiceKg = totalBatchKg * 0.05m;
        requiredSpiceKg.Should().Be(5.0m);

        // Required fat = 100 * 0.20 = 20.0 kg
        decimal requiredFatKg = totalBatchKg * 0.20m;
        requiredFatKg.Should().Be(20.0m);
    }

    [Fact]
    public void ReprocessingSimulation_MarinatedCut_ShouldAccountForMarinadeUptake()
    {
        // In marinated primal cuts, meat absorbs marinade and olive oil,
        // so yield increases (e.g. 108% yield)
        decimal rawMeatKg = 42.0m;
        decimal meatRatio = 84.0m;
        decimal totalBatchKg = rawMeatKg / (meatRatio / 100.0m); // 50 kg
        decimal yieldPct = 108.0m;

        decimal outputKg = totalBatchKg * (yieldPct / 100.0m);
        outputKg.Should().Be(54.0m);

        // Extended shelf life test (+10 days)
        var recipe = new ProcessingRecipe
        {
            RecipeCode = "RCP-SOSLU-01",
            RecipeName = "Kekikli Soslu Dana Kuşbaşı",
            Category = RecipeProductCategory.MarinatedPrimalCuts,
            ShelfLifeExtensionDays = 10,
            ExpectedYieldPercentage = 108.0m
        };

        var extendedExpiry = DateTime.UtcNow.AddDays(recipe.ShelfLifeExtensionDays);
        extendedExpiry.Should().BeAfter(DateTime.UtcNow.AddDays(9));
    }

    [Fact]
    public void CustomRecipe_ItemTypeEnum_SupportsSauceAndCuringAdditions()
    {
        var sauceType = RecipeItemType.SauceAndMarinade;
        var curingType = RecipeItemType.SaltAndCuring;

        ((int)sauceType).Should().Be(6);
        ((int)curingType).Should().Be(7);
    }

    [Theory]
    [InlineData("tr-TR", "Reyon SKT Kurtarma", "Soslu & Marine", "Gıda Güvenliği")]
    [InlineData("en-GB", "Display Shelf-Life Salvage", "Marinated", "Food Safety")]
    [InlineData("pl-PL", "System ratowania mięsa", "marynacie", "HACCP")]
    public void LocalizationService_ShouldProvideAccurateTranslationsAcrossLanguages(
        string langCode,
        string expectedTitleSubstring,
        string expectedCategoryOrMarinatedSubstring,
        string expectedHaccpSubstring)
    {
        var locService = new LocalizationService();
        locService.SetLanguage(langCode);

        locService.CurrentLanguageCode.Should().Be(langCode);

        var title = locService.Get("reprocessing.title");
        var marCategory = locService.Get("recipe.category.marinated");
        var haccpTitle = locService.Get("reprocessing.haccp.whyCuring");

        title.Should().NotBeNullOrEmpty();
        title.Should().Contain(expectedTitleSubstring);

        marCategory.Should().NotBeNullOrEmpty();
        marCategory.ToLower().Should().Contain(expectedCategoryOrMarinatedSubstring.ToLower());

        haccpTitle.Should().NotBeNullOrEmpty();
        haccpTitle.Should().Contain(expectedHaccpSubstring);
    }

    [Fact]
    public void IndustrialEquipment_SanitizationState_ShouldBeTrackable()
    {
        var eq = new MeatProcessingEquipment
        {
            EquipmentCode = "EQP-KYM-01",
            EquipmentName = "Soğutmalı Kıyma Makinesi",
            EquipmentType = "Kıyma Makinesi",
            OperatingStatus = "Hazır / Operasyonel",
            LastSanitizationDate = DateTime.UtcNow.AddHours(-1),
            SanitizedBy = "Mustafa Usta",
            SanitizingAgent = "Perasetik Asit (%0.2) + 82°C Sıcak Su",
            IsSanitizedAndReady = true
        };

        eq.EquipmentCode.Should().Be("EQP-KYM-01");
        eq.IsSanitizedAndReady.Should().BeTrue();
        eq.OperatingStatus.Should().Contain("Hazır");
        eq.SanitizingAgent.Should().Contain("Perasetik Asit");
    }

    [Fact]
    public void AuxiliaryMaterialStock_LowStockFlag_ShouldTriggerWhenAtOrBelowMinLevel()
    {
        var auxStock = new AuxiliaryMaterialStock
        {
            MaterialCode = "BAH-SCK-01",
            MaterialName = "Sucuk Baharatı",
            CurrentStock = 4.5m,
            MinStockLevel = 5.0m,
            UnitCost = 150.0m
        };

        auxStock.IsLowStock.Should().BeTrue();
        auxStock.TotalValue.Should().Be(675.00m); // 4.5 * 150

        // Replenish stock
        auxStock.CurrentStock = 20.0m;
        auxStock.IsLowStock.Should().BeFalse();
        auxStock.TotalValue.Should().Be(3000.00m);
    }

    [Theory]
    [InlineData(5.60, 0.88, 2.5, true, false)]  // Normal fresh meat pH
    [InlineData(5.75, 0.89, 3.2, true, false)]  // Normal range
    [InlineData(6.25, 0.94, 4.0, false, true)]  // pH > 6.20 critical CCP breach -> AUTO BLOCKED
    [InlineData(6.50, 0.96, 9.5, false, true)]  // High pH + high temp -> AUTO BLOCKED
    public void HaccpRule_PhOver620_ShouldTriggerAutoQuarantineAndBlock(
        decimal measuredPh,
        decimal measuredAw,
        decimal measuredTemp,
        bool expectedApproval,
        bool expectedAutoBlocked)
    {
        bool isCriticalPhExceeded = measuredPh > 6.20m;
        bool isTempTooHigh = measuredTemp > 8.0m;
        bool isApproved = !isCriticalPhExceeded && !isTempTooHigh;
        bool isAutoBlocked = isCriticalPhExceeded;

        measuredAw.Should().BeGreaterThan(0.50m);
        isApproved.Should().Be(expectedApproval);
        isAutoBlocked.Should().Be(expectedAutoBlocked);
    }

    [Fact]
    public void FinancialRoi_NetProfitFormula_ShouldAddValueAndPreventWaste()
    {
        // 100 kg expiring meat rescued (Cost: 380 TL/kg -> 38,000 TL potential waste)
        decimal rescuedMeatKg = 100.0m;
        decimal rawMeatCost = 380.0m;
        decimal totalWastePrevented = rescuedMeatKg * rawMeatCost; // 38,000 TL

        // Transformed into Sucuk with 90% yield = 90 kg
        decimal yieldPct = 90.0m;
        decimal finishedKg = rescuedMeatKg * (yieldPct / 100.0m); // 90 kg

        // Auxiliary spices & casings cost: 2,500 TL
        decimal auxCost = 2500.0m;
        decimal totalProductionCost = totalWastePrevented + auxCost; // 40,500 TL
        decimal newUnitCost = totalProductionCost / finishedKg; // 450 TL/kg

        // Sale price of artisan butcher sucuk: 650 TL/kg
        decimal salePricePerKg = 650.0m;
        decimal totalRevenue = finishedKg * salePricePerKg; // 90 * 650 = 58,500 TL

        decimal netProfit = totalRevenue - totalProductionCost; // 58,500 - 40,500 = 18,000 TL

        totalWastePrevented.Should().Be(38000.0m);
        finishedKg.Should().Be(90.0m);
        newUnitCost.Should().Be(450.0m);
        totalRevenue.Should().Be(58500.0m);
        netProfit.Should().Be(18000.0m);
    }

    [Theory]
    [InlineData("tr-TR", "Makine & Ekipman Parkı", "Baharat, Sos & Ambalaj Deposu", "Finansal Fire Önleme")]
    [InlineData("en-GB", "Machinery & Equipment", "Spices, Sauces & Packaging", "Financial Salvage")]
    [InlineData("pl-PL", "Park maszynowy", "Przyprawy, sosy", "Finansowy zwrot")]
    public void Localization_NewComprehensiveTabs_ShouldBePresentInTrEnPl(
        string langCode,
        string expectedEquipmentSubstring,
        string expectedAuxSubstring,
        string expectedRoiSubstring)
    {
        var locService = new LocalizationService();
        locService.SetLanguage(langCode);

        var tabEq = locService.Get("reprocessing.tab.equipment");
        var tabAux = locService.Get("reprocessing.tab.aux");
        var tabRoi = locService.Get("reprocessing.tab.roi");

        tabEq.Should().Contain(expectedEquipmentSubstring);
        tabAux.Should().Contain(expectedAuxSubstring);
        tabRoi.Should().Contain(expectedRoiSubstring);
    }

    [Fact]
    public void LegalThermalLabel_Generation_ShouldIncludeAllergensAndNutrition()
    {
        var label = new LegalThermalLabelDto
        {
            ProductName = "GELENEKSEL FERMENTE KASAP SUCUK",
            NetWeightKg = 0.850m,
            UnitPriceLira = 680.0m,
            LotNumber = "LOT-2026-SCK-88",
            Barcode = "8690001009841",
            PluCode = "2401",
            PackagingDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(60),
            IngredientsList = "Dana eti ve yağı, sarımsak, kimyon, tatlı kırmızı biber, kaya tuzu (alerjenler: HARDAL TOHUMU, GLUTEN).",
            AllergenWarning = "Alerjen Uyarısı: Hardal tohumu ve gluten içerir.",
            EnergyKcal = 310.0m,
            ProteinG = 19.8m,
            FatG = 24.5m,
            SaturatedFatG = 11.2m,
            SaltG = 2.4m,
            BusinessApprovalNo = "TR-34-K-009841",
            StorageCondition = "+0°C ile +4°C arasında muhafaza ediniz."
        };

        label.TotalPriceLira.Should().Be(578.00m); // 0.850 * 680
        label.IngredientsList.Should().Contain("HARDAL");
        label.AllergenWarning.Should().Contain("Hardal tohumu ve gluten");
        label.EnergyKcal.Should().Be(310.0m);
        label.BusinessApprovalNo.Should().StartWith("TR-");
        label.Barcode.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(480.0, 20.0, 384.0)]  // 480 TL with 20% discount = 384 TL
    [InlineData(500.0, 15.0, 425.0)]  // 500 TL with 15% discount = 425 TL
    [InlineData(600.0, 30.0, 420.0)]  // 600 TL with 30% discount = 420 TL
    public void SmartMarkdown_PriceCalculation_ShouldAccuratelyDiscount(decimal originalPrice, decimal discountPct, decimal expectedDiscountedPrice)
    {
        decimal discountAmount = originalPrice * (discountPct / 100.0m);
        decimal newPrice = originalPrice - discountAmount;

        newPrice.Should().Be(expectedDiscountedPrice);
    }

    [Fact]
    public void SucukCuringBatch_MoistureLossAndPhDrop_DetectsSaleReadiness()
    {
        var batch = new SucukCuringBatch
        {
            CuringLotNumber = "CUR-2026-001",
            InitialGreenWeightKg = 100.0m,
            CurrentWeightKg = 86.0m,  // 14% moisture loss
            TargetDryWeightKg = 88.0m,
            InitialPh = 5.65m,
            CurrentPh = 5.15m,        // Target <= 5.30 achieved!
            ElapsedDays = 14,
            TargetCuringDays = 14,
            ChamberTemperatureCelsius = 15.2m,
            ChamberHumidityRh = 78.0m
        };

        batch.CurrentMoistureLossPercentage.Should().Be(14.0m);
        batch.IsReadyForSale.Should().BeTrue();
        batch.Status.Should().Contain("Satışa Uygun");

        // Test an unready batch (only 6% loss, pH still 5.50)
        var unreadyBatch = new SucukCuringBatch
        {
            CuringLotNumber = "CUR-2026-002",
            InitialGreenWeightKg = 100.0m,
            CurrentWeightKg = 94.0m,
            TargetDryWeightKg = 88.0m,
            InitialPh = 5.65m,
            CurrentPh = 5.50m,
            ElapsedDays = 4,
            TargetCuringDays = 14
        };

        unreadyBatch.CurrentMoistureLossPercentage.Should().Be(6.0m);
        unreadyBatch.IsReadyForSale.Should().BeFalse();
        unreadyBatch.Status.Should().Contain("Fermantasyon");
    }

    [Theory]
    [InlineData(5, 5, 4, true, 4.7)]   // Excellent
    [InlineData(3, 4, 3, true, 3.3)]   // Passed minimal
    [InlineData(2, 4, 4, false, 3.3)]  // Smell score 2 < 3 -> FAIL
    [InlineData(4, 2, 4, false, 3.3)]  // Texture score 2 < 3 -> FAIL
    [InlineData(4, 4, 2, false, 3.3)]  // Color score 2 < 3 -> FAIL
    [InlineData(1, 1, 1, false, 1.0)]  // Completely spoiled -> FAIL
    public void OrganolepticSensoryCheck_ThresholdRules_ShouldVerifyMeatAcceptance(
        int smell, int texture, int color, bool expectedPass, decimal expectedAvg)
    {
        var check = new OrganolepticSensoryCheckDto
        {
            SmellScore = smell,
            TextureElasticityScore = texture,
            ColorAppearanceScore = color
        };

        check.IsPassed.Should().Be(expectedPass);
        check.AverageScore.Should().Be(expectedAvg);
    }

    [Fact]
    public void OfficialDisposalRecord_ProtocolGeneration_IncludesDoubleAuthorization()
    {
        var record = new ReprocessingDisposalRecord
        {
            ProtocolNumber = "IMHA-2026-0089",
            BatchLotNumber = "LOT-2026-KYM-99",
            ProductName = "Kokuşmuş Reyon Kıyması",
            DisposedQuantityKg = 18.5m,
            EstimatedFinancialLossLira = 7400.0m,
            DisposalReason = "Kritik pH 6.48 ve Kokuşma (Organoleptik Red)",
            MeasuredPh = 6.48m,
            FirstApprover = "Ahmet Yılmaz (Usta Kasap)",
            SecondApprover = "Dr. Vet. Mehmet Demir (HACCP Sorumlusu)",
            RenderingCompanyName = "Eko-Biyo Atık & Rendering San. Tic. A.Ş.",
            WaybillNumber = "IRS-2026-8812",
            DisposalStatus = "Resmi Bertaraf Edildi (Lisanslı Tesis)"
        };

        record.ProtocolNumber.Should().StartWith("IMHA-");
        record.DisposedQuantityKg.Should().Be(18.5m);
        record.FirstApprover.Should().Contain("Usta Kasap");
        record.SecondApprover.Should().Contain("HACCP");
        record.RenderingCompanyName.Should().Contain("Rendering");
        record.WaybillNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ButcherWasteIncentive_BonusCalculation_AccuratelyRewardsStaff()
    {
        var incentive = new ButcherWasteIncentive
        {
            ButcherName = "Ahmet Yılmaz",
            RoleTitle = "Şarküteri & Sucuk Şefi",
            TotalRescuedMeatKg = 165.0m,
            TotalBatchesReprocessed = 5,
            TotalNetValueCreatedLira = 74250.0m,
            IncentiveRatePercentage = 2.0m,
            BadgeTitle = "👑 Sıfır Fire Şampiyonu"
        };

        incentive.TotalRescuedMeatKg.Should().Be(165.0m);
        decimal calculatedBonus = incentive.TotalNetValueCreatedLira * (incentive.IncentiveRatePercentage / 100.0m);
        calculatedBonus.Should().Be(1485.0m);
        incentive.EarnedBonusLira.Should().Be(calculatedBonus);
        incentive.BadgeTitle.Should().Contain("Şampiyon");
    }

    [Theory]
    [InlineData("tr-TR", "Kurutma", "Duyusal", "İmha", "Primi")]
    [InlineData("en-GB", "Curing", "Sensory", "Disposal", "Incentive")]
    [InlineData("pl-PL", "dojrzewania", "organoleptyczna", "utylizacji", "Premia")]
    public void Localization_AdvancedModules_ShouldBeFullyTranslatedAcrossLanguages(
        string langCode,
        string expectedCuring,
        string expectedSensory,
        string expectedDisposal,
        string expectedIncentive)
    {
        var locService = new LocalizationService();
        locService.SetLanguage(langCode);

        var curingTab = locService.Get("reprocessing.tab.curing");
        var sensoryTab = locService.Get("reprocessing.tab.sensory");
        var disposalTab = locService.Get("reprocessing.tab.disposal");
        var incentiveTab = locService.Get("reprocessing.tab.incentives");

        curingTab.ToLower().Should().Contain(expectedCuring.ToLower());
        sensoryTab.ToLower().Should().Contain(expectedSensory.ToLower());
        disposalTab.ToLower().Should().Contain(expectedDisposal.ToLower());
        incentiveTab.ToLower().Should().Contain(expectedIncentive.ToLower());
    }
}

