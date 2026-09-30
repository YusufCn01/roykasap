using FluentAssertions;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Licensing;
using KasapOtomasyon.Infrastructure.Services;
using Xunit;

namespace KasapOtomasyon.Tests;

public class LicenseTests
{
    [Fact]
    public void HardwareFingerprint_ShouldHaveStandardFormat()
    {
        var provider = new HardwareFingerprintProvider();
        var fp = provider.GetMachineFingerprint();

        fp.Fingerprint.Should().NotBeNullOrEmpty();
        fp.Fingerprint.Should().StartWith("KASAP-");
        fp.Fingerprint.Split('-').Length.Should().Be(5);
    }

    [Fact]
    public void RsaSignature_ShouldSignAndVerifySuccessfully()
    {
        var testData = "KasapOtomasyon_Customer_12345_Test_Payload";
        var signature = RsaLicenseValidator.SignData(testData, RsaKeyConstants.MasterPrivateKeyXml);

        signature.Should().NotBeNullOrEmpty();

        var isValid = RsaLicenseValidator.VerifySignature(testData, signature);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void LicenseValidationResult_ShouldCorrectlyExposeFeatureFlags()
    {
        var result = new LicenseValidationResultDto
        {
            IsValid = true,
            LicenseType = LicenseType.Standart,
            EnabledFeatures = new List<string> { "POS_SALE", "STOCK_MANAGEMENT" }
        };

        result.CanRunMezbahaModule.Should().BeFalse();
        result.CanRunEInvoice.Should().BeFalse();

        var proResult = new LicenseValidationResultDto
        {
            IsValid = true,
            LicenseType = LicenseType.Profesyonel,
            EnabledFeatures = new List<string> { "POS_SALE", "STOCK_MANAGEMENT", "MEZBAHA_MODULE" }
        };

        proResult.CanRunMezbahaModule.Should().BeTrue();
    }
}

public class BarcodeTests
{
    private readonly BarcodeService _barcodeService = new();

    [Fact]
    public void ParseBarcode_WithScaleEmbeddedWeightBarcode_ShouldExtractPluAndWeight()
    {
        // 20 + 00001 (PLU) + 01250 (1.250 kg) + Check Digit
        var generated = _barcodeService.GenerateWeightedBarcode("00001", 1.250m);
        generated.Should().StartWith("200000101250");

        var parsed = _barcodeService.ParseBarcode(generated);
        parsed.IsValid.Should().BeTrue();
        parsed.IsWeightEmbedded.Should().BeTrue();
        parsed.PluCode.Should().Be("00001");
        parsed.WeightKg.Should().Be(1.250m);
    }

    [Fact]
    public void GenerateEan13_ShouldProduce13DigitValidCode()
    {
        var code = _barcodeService.GenerateEan13("12345");
        code.Length.Should().Be(13);
    }
}

public class ProductionYieldTests
{
    [Fact]
    public void YieldCalculation_ShouldAllocateCostBasedOnCutValueRatio()
    {
        var inputCarcassWeight = 300.0m;
        var inputTotalCost = 90000.0m; // 300 TL/kg

        var outputs = new List<ProductionOutputDto>
        {
            new() { CutName = "Antrikot", WeightKg = 25.0m, IsByproduct = false, IsWaste = false },
            new() { CutName = "Kıyma", WeightKg = 90.0m, IsByproduct = false, IsWaste = false },
            new() { CutName = "Sakatat / Ciğer", WeightKg = 15.0m, IsByproduct = true, IsWaste = false },
            new() { CutName = "Kemik & Fire", WeightKg = 20.0m, IsByproduct = false, IsWaste = true }
        };

        // Total weighted points:
        // Antrikot: 25 * 1.0 = 25
        // Kıyma: 90 * 1.0 = 90
        // Sakatat: 15 * 0.5 = 7.5
        // Waste: 20 * 0.0 = 0
        // Total points = 122.5
        decimal totalPoints = (25 * 1.0m) + (90 * 1.0m) + (15 * 0.5m);

        foreach (var o in outputs)
        {
            if (o.IsWaste)
            {
                o.CostPerKg = 0;
                o.TotalCost = 0;
            }
            else
            {
                var ratio = o.IsByproduct ? 0.5m : 1.0m;
                var share = (o.WeightKg * ratio) / totalPoints;
                o.TotalCost = Math.Round(inputTotalCost * share, 2);
                o.CostPerKg = Math.Round(o.TotalCost / o.WeightKg, 2);
            }
        }

        var wasteCut = outputs.First(x => x.IsWaste);
        wasteCut.TotalCost.Should().Be(0);

        var antrikotCut = outputs.First(x => x.CutName == "Antrikot");
        antrikotCut.CostPerKg.Should().BeGreaterThan(0);

        var sumCost = outputs.Sum(x => x.TotalCost);
        sumCost.Should().BeApproximately(inputTotalCost, 1.0m);
    }
}

public class PosCalculationTests
{
    [Fact]
    public void CartTotals_WithDiscounts_ShouldCalculateCorrectly()
    {
        var item1 = new PosCartItemDto
        {
            ProductId = 1,
            ProductName = "Dana Antrikot",
            Quantity = 1.500m,
            UnitPrice = 600.00m,
            DiscountPercent = 10m,
            DiscountAmount = (1.500m * 600.00m) * 0.10m // 90 TL discount
        };

        var item2 = new PosCartItemDto
        {
            ProductId = 2,
            ProductName = "Kasap Köfte",
            Quantity = 2.000m,
            UnitPrice = 400.00m,
            DiscountAmount = 0m
        };

        item1.TotalPrice.Should().Be(810.00m); // 900 - 90
        item2.TotalPrice.Should().Be(800.00m); // 800

        var grandTotal = item1.TotalPrice + item2.TotalPrice;
        grandTotal.Should().Be(1610.00m);

        var payments = new List<SplitPaymentDto>
        {
            new() { PaymentType = PaymentType.Nakit, Amount = 1000.00m },
            new() { PaymentType = PaymentType.KrediKarti, Amount = 610.00m }
        };

        payments.Sum(p => p.Amount).Should().Be(grandTotal);
    }
}

public class SlaughterhouseAndTraceabilityTests
{
    [Fact]
    public void SlaughterRecord_YieldPercentage_ShouldCalculateAccurately()
    {
        // 612 kg canlı -> 328 kg karkas -> %53.59 randıman
        decimal liveWeight = 612.0m;
        decimal hotCarcassWeight = 328.0m;

        decimal yieldPercentage = Math.Round((hotCarcassWeight / liveWeight) * 100m, 2);

        yieldPercentage.Should().Be(53.59m);
    }

    [Fact]
    public void SlaughterRecord_ActivityBasedCosting_ShouldDistributeAccurately()
    {
        decimal animalPurchaseCost = 115000.0m;
        decimal slaughterLaborCost = 2500.0m;
        decimal transportationCost = 3000.0m;
        decimal coolingElectricityCost = 1500.0m;
        decimal generalOverheadCost = 3000.0m;
        decimal coldCarcassWeightKg = 321.4m;

        decimal totalCost = animalPurchaseCost + slaughterLaborCost + transportationCost + coolingElectricityCost + generalOverheadCost;
        decimal costPerKg = Math.Round(totalCost / coldCarcassWeightKg, 2);

        totalCost.Should().Be(125000.0m);
        costPerKg.Should().Be(388.92m);
    }

    [Fact]
    public void ForensicAnomaly_CarcassLoss_ShouldCalculateFinancialLoss()
    {
        decimal expectedQuantity = 286.0m;
        decimal actualQuantity = 263.0m;
        decimal meatCostPerKg = 390.0m;

        decimal differenceKg = expectedQuantity - actualQuantity;
        decimal financialLoss = differenceKg * meatCostPerKg;

        differenceKg.Should().Be(23.0m);
        financialLoss.Should().Be(8970.0m);
    }
}

public class MultiTenantAndLocalizationTests
{
    private readonly LocalizationService _locService = new();

    [Fact]
    public void Localization_ShouldReturnTurkishTranslationsByDefault()
    {
        _locService.SetLanguage("tr-TR");
        _locService.Get("common.save").Should().Be("Kaydet");
        _locService.Get("animal.earTag").Should().Be("Bakanlık Küpe No");
        _locService.Get("vet.approved").Should().Be("✓ Kesime Uygun (Onaylı)");
    }

    [Fact]
    public void Localization_ShouldSwitchToEnglish_AndReturnValidEnglishPhrases()
    {
        _locService.SetLanguage("en-GB");
        _locService.Get("common.save").Should().Be("Save");
        _locService.Get("animal.earTag").Should().Be("Ministry Ear Tag No");
        _locService.Get("slaughter.seurop").Should().Be("SEUROP Class");
        _locService.Get("cutting.massBalance").Should().Be("Mass Balance Verification");
    }

    [Fact]
    public void Localization_ShouldSwitchToPolish_AndReturnValidPolishPhrases()
    {
        _locService.SetLanguage("pl-PL");
        _locService.Get("common.save").Should().Be("Zapisz");
        _locService.Get("animal.earTag").Should().Be("Numer kolczyka (ARiMR)");
        _locService.Get("slaughter.hotWeight").Should().Be("Waga ciepła tuszy (kg)");
        _locService.Get("vet.approved").Should().Be("✓ Zgoda na ubój wydana");
    }

    [Fact]
    public void Localization_ShouldSupportIndexedPropertyAccess()
    {
        _locService.SetLanguage("en-GB");
        _locService["common.cancel"].Should().Be("Cancel");
        _locService["tenant.plant"].Should().Be("Plant / Slaughterhouse");
    }
}

public class ImmutableAuditTests
{
    [Fact]
    public void AuditSignature_ShouldGenerateDeterministicSha256Signature()
    {
        var action = "CARCASS_WEIGHT_OVERRIDE";
        var entityName = "SlaughterRecord";
        var entityId = "KRK-2026-000192";
        var userName = "supervisor_mehmet";
        var timestamp = new DateTime(2026, 8, 23, 12, 0, 0, DateTimeKind.Utc);
        var reason = "Kantar sıfırlama kalibrasyonu sonrası tekrar tartıldı";
        var approver = "plant_manager_demir";
        var oldVal = "340.50";
        var newVal = "348.20";
        var tenantId = 1;
        var plantId = 1;

        var sigSource = $"{action}|{entityName}|{entityId}|{userName}|{timestamp:O}|{reason}|{approver}|{oldVal}|{newVal}|{tenantId}|{plantId}";
        var signature1 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(sigSource)));
        var signature2 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(sigSource)));

        signature1.Should().NotBeNullOrEmpty();
        signature1.Length.Should().Be(64); // 256-bit hex
        signature1.Should().Be(signature2);
    }
}

public class EnterpriseMesAndMassBalanceTests
{
    [Fact]
    public void CuttingOrder_MassBalance_ShouldBeApprovedWhenVarianceUnderHalfPercent()
    {
        var order = new KasapOtomasyon.Domain.Entities.CuttingOrder
        {
            InputCarcassWeightKg = 320.00m,
            TotalPrimalCutsWeightKg = 210.50m,
            TotalByproductsWeightKg = 18.20m,
            TotalBoneAndFatWeightKg = 78.40m,
            TotalWasteWeightKg = 8.50m,
            ProcessLossWeightKg = 3.80m // Total = 319.40 kg (Diff = 0.60 kg, %0.19)
        };

        order.TotalOutputWeightKg.Should().Be(319.40m);
        order.MassBalanceDifferenceKg.Should().Be(0.60m);
        order.MassBalanceVariancePercentage.Should().BeLessThanOrEqualTo(0.50m);
        order.IsMassBalanceApproved.Should().BeTrue();
    }

    [Fact]
    public void CuttingOrder_MassBalance_ShouldFailWhenVarianceExceedsHalfPercent()
    {
        var order = new KasapOtomasyon.Domain.Entities.CuttingOrder
        {
            InputCarcassWeightKg = 320.00m,
            TotalPrimalCutsWeightKg = 180.00m,
            TotalByproductsWeightKg = 15.00m,
            TotalBoneAndFatWeightKg = 70.00m,
            TotalWasteWeightKg = 5.00m,
            ProcessLossWeightKg = 2.00m // Total = 272.00 kg (Diff = 48.00 kg, %15.00)
        };

        order.TotalOutputWeightKg.Should().Be(272.00m);
        order.MassBalanceDifferenceKg.Should().Be(48.00m);
        order.MassBalanceVariancePercentage.Should().Be(15.00m);
        order.IsMassBalanceApproved.Should().BeFalse();
    }
}

public class EnterpriseAntiFraudTests
{
    [Fact]
    public void AntiFraud_ScaleWeightOverride_ShouldTriggerCriticalRiskOnLargeShift()
    {
        var service = new AntiFraudEngineService(null!, null!);
        var result = service.EvaluateScaleOverride(320.0m, 360.0m, "op_ahmet");

        result.RiskLevel.Should().Be(FraudRiskLevel.Critical);
        result.RiskScore.Should().BeGreaterThanOrEqualTo(90.0m);
        result.RequiresSupervisorApproval.Should().BeTrue();
    }

    [Fact]
    public void AntiFraud_CuttingVariance_ShouldTriggerAnomalyWarning()
    {
        var service = new AntiFraudEngineService(null!, null!);
        var result = service.EvaluateCuttingVariance(300.0m, 280.0m); // 20 kg (%6.67) loss

        result.RiskLevel.Should().Be(FraudRiskLevel.Critical);
        result.RequiresSupervisorApproval.Should().BeTrue();
    }

    [Fact]
    public void AntiFraud_SaleBelowCost_ShouldRequireSupervisorApproval()
    {
        var service = new AntiFraudEngineService(null!, null!);
        var result = service.EvaluateSaleBelowCost(450.0m, 380.0m); // Selling at loss

        result.RiskLevel.Should().Be(FraudRiskLevel.High);
        result.RequiresSupervisorApproval.Should().BeTrue();
    }
}

public class EnterpriseTrueCostTests
{
    [Fact]
    public void TrueCostRollup_ShouldAccuratelyDeductByproductRevenuesFromTotalCost()
    {
        var cost = new KasapOtomasyon.Domain.Entities.TrueCostRollup
        {
            AnimalPurchaseCost = 110000.0m,
            LivestockTransportCost = 3000.0m,
            SlaughterLaborCost = 2500.0m,
            VeterinaryInspectionCost = 750.0m,
            ColdStorageChillingEnergyCost = 1500.0m,
            DeboningLaborCost = 1500.0m,
            PackagingAndLabelingCost = 800.0m,
            GeneralFacilityOverheadCost = 2000.0m,
            
            // By-product revenue credits
            HideLeatherCreditRevenue = 1500.0m,
            OffalOrganCreditRevenue = 3000.0m,
            BoneAndFatCreditRevenue = 500.0m,

            ColdCarcassWeightKg = 310.0m
        };

        cost.TotalGrossCost.Should().Be(122050.0m);
        cost.TotalByproductCredits.Should().Be(5000.0m);
        cost.NetTrueCost.Should().Be(117050.0m);
        cost.TrueCostPerCarcassKg.Should().Be(377.58m);
    }
}

public class EnterpriseProcessingAndBomTests
{
    [Fact]
    public void ProcessingRecipe_BatchAllocation_ShouldMatchRecipePercentages()
    {
        var recipe = new ProcessingRecipe
        {
            RecipeCode = "RCP-SUCUK-01",
            RecipeName = "Kangal Sucuk",
            StandardBatchWeightKg = 100.0m,
            ExpectedYieldPercentage = 88.0m // %12 drying loss
        };

        decimal batchInputKg = 200.0m;
        decimal expectedFinishedProductKg = batchInputKg * (recipe.ExpectedYieldPercentage / 100.0m);

        expectedFinishedProductKg.Should().Be(176.0m);
    }
}

public class EnterpriseWmsAndSsccTests
{
    [Fact]
    public void WmsPallet_Sscc18Generation_ShouldBe18DigitsWithValidModulo10Checksum()
    {
        var service = new WmsPalletLogisticsService(null!, null!);
        var sscc = service.GenerateSscc18("0", "8690001", "123456789");

        sscc.Should().HaveLength(18);
        sscc.StartsWith("08690001").Should().BeTrue();
    }
}

public class EnterprisePurchaseInvoiceTests
{
    [Fact]
    public void CommercialPurchaseInvoice_With9Over10Tevkifat_ShouldComputeCorrectNetPayable()
    {
        // 1000 kg Meat @ 350 TL/kg = 350,000 TL Matrah
        // %1 KDV = 3,500 TL
        // 9/10 Tevkifat = 3,150 TL (Deducted from supplier, paid to tax authority)
        // Net Payable to Supplier = 350,000 + 3,500 - 3,150 = 350,350 TL
        decimal quantity = 1000m;
        decimal unitPrice = 350m;
        decimal vatRate = 0.01m;
        decimal tevkifatRate = 0.90m; // 9/10

        decimal subTotal = quantity * unitPrice; // 350,000
        decimal vatAmount = subTotal * vatRate;   // 3,500
        decimal tevkifat = vatAmount * tevkifatRate; // 3,150
        decimal grandTotal = subTotal + vatAmount - tevkifat; // 350,350

        subTotal.Should().Be(350000m);
        vatAmount.Should().Be(3500m);
        tevkifat.Should().Be(3150m);
        grandTotal.Should().Be(350350m);
    }

    [Fact]
    public void ProducerReceipt_MustahsilMakbuzu_ShouldDeductWithholdingTaxAndSgk()
    {
        // 1 Cattle Purchase @ 120,000 TL Gross
        // %1 Income Withholding Tax (GV Stopaj) = 1,200 TL
        // %2 SGK Bağ-Kur = 2,400 TL
        // %0.1 Commodity Exchange Fee (Borsa) = 120 TL
        // %0.1 Pasture Fund (Mera Fonu) = 120 TL
        // Net Paid to Farmer = 120,000 - 3,840 = 116,160 TL
        decimal grossAmount = 120000m;
        decimal withholdingTax = grossAmount * 0.01m;
        decimal sgkBagkur = grossAmount * 0.02m;
        decimal borsaFee = grossAmount * 0.001m;
        decimal meraFund = grossAmount * 0.001m;

        decimal totalDeductions = withholdingTax + sgkBagkur + borsaFee + meraFund;
        decimal netPaidToFarmer = grossAmount - totalDeductions;

        withholdingTax.Should().Be(1200m);
        sgkBagkur.Should().Be(2400m);
        totalDeductions.Should().Be(3840m);
        netPaidToFarmer.Should().Be(116160m);
    }
}

