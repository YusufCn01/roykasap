using KasapOtomasyon.Domain.Entities;

namespace KasapOtomasyon.Application.Interfaces;

public class MassBalanceCalculationResult
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal InputWeightKg { get; set; }
    public decimal OutputWeightKg { get; set; }
    public decimal DifferenceKg { get; set; }
    public decimal VariancePercentage { get; set; }
    public bool IsApproved { get; set; }
    public string Message { get; set; } = string.Empty;
}

public interface IMassBalanceAndCuttingService
{
    Task<CuttingOrder> CreateCuttingOrderAsync(string carcassNumber, decimal inputWeightKg, string masterButcher);
    Task<MassBalanceCalculationResult> ValidateMassBalanceAsync(int cuttingOrderId);
    Task<CuttingOrder> FinalizeCuttingOrderAsync(int cuttingOrderId, List<CarcassDeboningCut> cuts, decimal boneKg, decimal fatKg, decimal wasteKg, decimal processLossKg);
    Task<List<CuttingOrder>> GetCuttingOrdersAsync(int plantId = 1);
}

public class FoodRecallAnalysisResult
{
    public string CaseNumber { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public int AffectedLotsCount { get; set; }
    public int AffectedCustomersCount { get; set; }
    public decimal TotalRecalledWeightKg { get; set; }
    public decimal EstimatedFinancialLoss { get; set; }
    public List<FoodRecallItem> AffectedItems { get; set; } = new();
}

public interface IFoodRecallAndHaccpService
{
    Task<FoodRecallAnalysisResult> Trigger1ClickRecallAsync(string rootCauseEarTagOrCarcass, string triggerReason);
    Task<HaccpInspectionRecord> RecordHaccpMeasurementAsync(int ccpId, string targetIdentifier, decimal measuredValue, string inspectorName);
    Task<List<FoodRecallCase>> GetActiveRecallCasesAsync();
    Task<bool> ResolveRecallCaseAsync(int caseId, string resolutionNotes);
}

public interface ITrueCostEngineService
{
    Task<TrueCostRollup> CalculateTrueCostAsync(string carcassNumber);
    Task<List<TrueCostRollup>> GetPlantCostRollupsAsync(int plantId = 1);
}

public class FraudRiskEvaluationResult
{
    public FraudRiskLevel RiskLevel { get; set; } = FraudRiskLevel.Low;
    public decimal RiskScore { get; set; } // 0 - 100
    public bool RequiresSupervisorApproval { get; set; }
    public string WarningMessage { get; set; } = string.Empty;
}

public interface IAntiFraudEngineService
{
    FraudRiskEvaluationResult EvaluateScaleOverride(decimal oldWeightKg, decimal newWeightKg, string operatorName);
    FraudRiskEvaluationResult EvaluateCuttingVariance(decimal inputKg, decimal outputKg);
    FraudRiskEvaluationResult EvaluateSaleBelowCost(decimal unitCost, decimal unitPrice);
    Task<FraudInvestigationCase> CreateFraudCaseAsync(string action, string suspectedUser, decimal lossAmount, string evidence);
    Task<List<FraudInvestigationCase>> GetOpenFraudCasesAsync();
}

public interface IMeatProcessingService
{
    Task<List<ProcessingRecipe>> GetRecipesAsync(int plantId = 1);
    Task<ProcessingRecipe> CreateRecipeAsync(ProcessingRecipe recipe, List<ProcessingRecipeItem> items);
    Task<ProductionOrder> ExecuteProcessingBatchAsync(int recipeId, decimal targetBatchWeightKg, string operatorName);
}

public interface IWmsPalletLogisticsService
{
    Task<PalletSSCC> BuildPalletSSCCAsync(string productName, string lotNumber, int boxCount, decimal netWeightKg, string warehouseBin, string customer = "");
    Task<List<PalletSSCC>> GetActivePalletsAsync(int plantId = 1);
    Task<bool> QuarantinePalletAsync(int palletId, string reason);
    string GenerateSscc18(string extensionDigit, string gcp, string serialNumber);
}
