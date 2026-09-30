using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.DTOs;

public class AnimalLotDto
{
    public int Id { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public AnimalType AnimalType { get; set; }
    public AnimalCarcassType CarcassType { get; set; }
    public string? SupplierName { get; set; }
    public string? EarTagNumber { get; set; }
    public decimal LiveWeightKg { get; set; }
    public decimal CarcassWeightKg { get; set; }
    public decimal PurchasePriceTotal { get; set; }
    public decimal UnitCostPerKg { get; set; }
    public string? Origin { get; set; }
    public DateTime SlaughterDate { get; set; }
    public string? VeterinaryReportNo { get; set; }
    public string? TraceabilityCode { get; set; }
    public bool IsProcessed { get; set; }
}

public class CuttingTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AnimalType AnimalType { get; set; }
    public string? Description { get; set; }
    public List<CuttingTemplateItemDto> Items { get; set; } = new();
}

public class CuttingTemplateItemDto
{
    public int Id { get; set; }
    public string CutName { get; set; } = string.Empty;
    public int? TargetProductId { get; set; }
    public string? TargetProductName { get; set; }
    public decimal ExpectedPercentage { get; set; }
    public decimal CostWeightRatio { get; set; } = 1.0m;
    public bool IsByproduct { get; set; }
    public bool IsWaste { get; set; }
}

public class ProductionOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int LotId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public string AnimalTypeStr { get; set; } = string.Empty;
    public int? CuttingTemplateId { get; set; }
    public string? CuttingTemplateName { get; set; }
    public DateTime OrderDate { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
    public decimal InputWeightKg { get; set; }
    public decimal InputCostTotal { get; set; }
    public decimal TotalOutputWeightKg { get; set; }
    public decimal TotalYieldPercentage { get; set; }
    public decimal TotalWasteWeightKg { get; set; }
    public ProductionOrderStatus Status { get; set; }
    public List<ProductionOutputDto> Outputs { get; set; } = new();
}

public class ProductionOutputDto
{
    public int? ProductId { get; set; }
    public string CutName { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal TotalCost { get; set; }
    public decimal YieldPercentage { get; set; }
    public string? BatchBarcode { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsWaste { get; set; }
    public bool IsByproduct { get; set; }
}

public class CuttingYieldReportDto
{
    public string LotNumber { get; set; } = string.Empty;
    public string AnimalInfo { get; set; } = string.Empty;
    public decimal InputCarcassWeightKg { get; set; }
    public decimal InputTotalCost { get; set; }
    public decimal TotalEdibleWeightKg { get; set; } // Yenilebilir etler
    public decimal TotalByproductWeightKg { get; set; } // Sakatat
    public decimal TotalWasteWeightKg { get; set; } // Kemik / Yağ / Nem Fire
    public decimal NetMeatYieldPercentage { get; set; } // Net Et Randımanı %
    public decimal WastePercentage { get; set; } // Fire %
    public List<ProductionOutputDto> CutsBreakdown { get; set; } = new();
}

public class StockItemDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal CurrentQuantity { get; set; }
    public decimal MinStockLevel { get; set; }
    public decimal MaxStockLevel { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsCriticalStock => CurrentQuantity <= MinStockLevel;
    public bool IsCriticalExpiry => ExpiryDate.HasValue && (ExpiryDate.Value - DateTime.UtcNow).TotalDays <= 2;
}
