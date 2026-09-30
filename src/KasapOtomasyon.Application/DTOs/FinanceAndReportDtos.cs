using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxNumberOrId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public bool IsOverdue { get; set; }
    public decimal OverdueBalance { get; set; }
}

public class CashSessionSummaryDto
{
    public int SessionId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalSalesCash { get; set; }
    public decimal TotalSalesCreditCard { get; set; }
    public decimal TotalSalesOnAccount { get; set; }
    public decimal TotalCashIn { get; set; }
    public decimal TotalCashOut { get; set; }
    public decimal CalculatedBalance { get; set; }
    public decimal? ActualBalance { get; set; }
    public decimal? DiscrepancyAmount { get; set; }
    public string Status { get; set; } = "Acik";
}

public class DashboardSummaryDto
{
    public decimal TodayTurnover { get; set; }
    public int TodaySaleCount { get; set; }
    public decimal TodayCashTotal { get; set; }
    public decimal TodayCreditCardTotal { get; set; }
    public decimal OpenCashBalance { get; set; }
    public int CriticalStockCount { get; set; }
    public int CriticalExpiryCount { get; set; }
    public decimal ActiveLotCount { get; set; }
    public decimal AverageProductionYield { get; set; }
    
    // Enterprise Meat ERP & Slaughterhouse KPIs
    public int TotalAnimalsSlaughtered { get; set; }
    public decimal TotalCarcassWeightKg { get; set; }
    public decimal AverageCarcassYieldPercentage { get; set; }
    public decimal MassBalanceComplianceRate { get; set; } = 99.4m;
    public int ActiveHaccpAlertsCount { get; set; }
    public int TotalPalletsInWms { get; set; }

    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    public List<DailyTurnoverReportDto> Last7DaysTurnover { get; set; } = new();
    public List<StockItemDto> CriticalExpiryProducts { get; set; } = new();
}

public class DailyTurnoverReportDto
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal CardSales { get; set; }
    public decimal OnAccountSales { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalProfit { get; set; }
}

public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantitySold { get; set; }
    public string UnitName { get; set; } = "KG";
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
}

public class ProfitabilityReportDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal GrossProfit => Revenue - Cost;
    public decimal ProfitMarginPercentage => Revenue > 0 ? Math.Round((GrossProfit / Revenue) * 100m, 2) : 0;
}

public class CariAgingReportDto
{
    public decimal TotalReceivable { get; set; }
    public decimal OverdueReceivable { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Days90Plus { get; set; }
    public List<CustomerAgingItemDto> CustomerBreakdowns { get; set; } = new();
}

public class CustomerAgingItemDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OverdueDays { get; set; }
    public string RiskLevel { get; set; } = "Düşük"; // Düşük, Orta, Yüksek, Kritik
}

public class CarcassYieldByBreedReportDto
{
    public string BreedName { get; set; } = string.Empty;
    public string Category { get; set; } = "Büyükbaş";
    public int TotalAnimals { get; set; }
    public decimal AvgLiveWeightKg { get; set; }
    public decimal AvgColdCarcassWeightKg { get; set; }
    public decimal AvgYieldPercentage { get; set; }
    public decimal BestYieldPercentage { get; set; }
    public string DominantSeuropClass { get; set; } = "R";
}

public class DeboningMassBalanceReportDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ProcessDate { get; set; }
    public decimal InputCarcassWeightKg { get; set; }
    public decimal PrimalCutsWeightKg { get; set; }
    public decimal ByproductsWeightKg { get; set; }
    public decimal BonesAndFatWeightKg { get; set; }
    public decimal WasteWeightKg { get; set; }
    public decimal TotalOutputWeightKg => PrimalCutsWeightKg + ByproductsWeightKg + BonesAndFatWeightKg + WasteWeightKg;
    public decimal VarianceWeightKg => Math.Abs(InputCarcassWeightKg - TotalOutputWeightKg);
    public decimal VariancePercentage => InputCarcassWeightKg > 0 ? Math.Round((VarianceWeightKg / InputCarcassWeightKg) * 100m, 2) : 0;
    public bool IsCompliant => VariancePercentage <= 0.50m;
}

public class HaccpQualityComplianceReportDto
{
    public string CcpCode { get; set; } = string.Empty;
    public string CcpName { get; set; } = string.Empty;
    public string CriticalLimitText { get; set; } = string.Empty;
    public int TotalInspections { get; set; }
    public int ViolationCount { get; set; }
    public decimal ComplianceRatePercentage => TotalInspections > 0 ? Math.Round(((TotalInspections - ViolationCount) / (decimal)TotalInspections) * 100m, 1) : 100m;
    public string StatusText => ViolationCount == 0 ? "Tam Uyumlu (Güvenli)" : $"{ViolationCount} Sapma Tespit Edildi";
}

