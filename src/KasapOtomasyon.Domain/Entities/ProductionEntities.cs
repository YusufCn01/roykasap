using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class AnimalLot : BaseEntity
{
    public string LotNumber { get; set; } = string.Empty; // LOT-2026-001
    public AnimalType AnimalType { get; set; } = AnimalType.Buyukbas;
    public AnimalCarcassType CarcassType { get; set; } = AnimalCarcassType.Karkas;
    public string? SupplierName { get; set; }
    public string? EarTagNumber { get; set; } // Hayvan Küpe No (TR340012345)
    public decimal LiveWeightKg { get; set; } // Canlı Ağırlık
    public decimal CarcassWeightKg { get; set; } // Karkas Ağırlığı
    public decimal PurchasePriceTotal { get; set; } // Alış Toplam Tutarı
    public decimal UnitCostPerKg => CarcassWeightKg > 0 ? Math.Round(PurchasePriceTotal / CarcassWeightKg, 2) : 0;
    public string? Origin { get; set; } // Menşei / Besi Çiftliği
    public DateTime SlaughterDate { get; set; } = DateTime.UtcNow;
    public string? VeterinaryReportNo { get; set; }
    public string? TraceabilityCode { get; set; } // Karekod / İzlenebilirlik Kodu
    public string? Notes { get; set; }
    public bool IsProcessed { get; set; }

    public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
}

public class CuttingTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g. "Dana Standart Parçalama Reçetesi", "Kuzu Bütün Parçalama"
    public AnimalType AnimalType { get; set; } = AnimalType.Buyukbas;
    public string? Description { get; set; }

    public virtual ICollection<CuttingTemplateItem> Items { get; set; } = new List<CuttingTemplateItem>();
    public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
}

public class CuttingTemplateItem : BaseEntity
{
    public int CuttingTemplateId { get; set; }
    public virtual CuttingTemplate CuttingTemplate { get; set; } = null!;
    
    public string CutName { get; set; } = string.Empty; // e.g. "Antrikot", "Bonfile", "Kuşbaşı / Kol", "Kıyma / Döş", "Pirzola", "Sakatat / Karaciğer", "Kemik", "Yağ", "Fire (Nem Kaybı)"
    public int? TargetProductId { get; set; } // Eşleşen Nihai Ürün
    public virtual Product? TargetProduct { get; set; }
    
    public decimal ExpectedPercentage { get; set; } // Beklenen Ağırlık Oranı % (Örn. %8 Antrikot, %25 Kıyma, %4 Fire)
    public decimal CostWeightRatio { get; set; } = 1.0m; // Değerli etler için maliyet çarpanı (Örn: Bonfile için 2.5x, Kıyma için 1.0x, Kemik için 0.1x)
    public bool IsByproduct { get; set; } // Sakatat / Yan Ürün mü?
    public bool IsWaste { get; set; } // Fire / Atık mı?
}

public class ProductionOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // URT-2026-0001
    
    public int LotId { get; set; }
    public virtual AnimalLot AnimalLot { get; set; } = null!;

    public int? CuttingTemplateId { get; set; }
    public virtual CuttingTemplate? CuttingTemplate { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string ResponsiblePerson { get; set; } = string.Empty;

    public decimal InputWeightKg { get; set; }
    public decimal InputCostTotal { get; set; }

    public decimal TotalOutputWeightKg { get; set; }
    public decimal TotalOutputCostTotal { get; set; }
    
    public decimal TotalWasteWeightKg { get; set; }
    public decimal TotalYieldPercentage => InputWeightKg > 0 ? Math.Round((TotalOutputWeightKg / InputWeightKg) * 100m, 2) : 0; // Randıman %
    
    public ProductionOrderStatus Status { get; set; } = ProductionOrderStatus.Planlandi;
    public string? Notes { get; set; }

    public virtual ICollection<ProductionInput> Inputs { get; set; } = new List<ProductionInput>();
    public virtual ICollection<ProductionOutput> Outputs { get; set; } = new List<ProductionOutput>();
}

public class ProductionInput : BaseEntity
{
    public int ProductionOrderId { get; set; }
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;

    public string ItemName { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => Math.Round(WeightKg * UnitCost, 2);
}

public class ProductionOutput : BaseEntity
{
    public int ProductionOrderId { get; set; }
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;

    public int? ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public string CutName { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal TotalCost => Math.Round(WeightKg * CostPerKg, 2);
    public decimal YieldPercentage { get; set; } // Toplam girdi içindeki gerçekleşen randıman %
    
    public string? BatchBarcode { get; set; } // Üretim Partisi Barkodu
    public DateTime? ExpiryDate { get; set; } // SKT
    public bool IsWaste { get; set; }
    public bool IsByproduct { get; set; }
}
