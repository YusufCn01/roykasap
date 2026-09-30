namespace KasapOtomasyon.Domain.Entities;

public class MeatProcessingEquipment : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string EquipmentCode { get; set; } = string.Empty; // EQP-KYM-01
    public string EquipmentName { get; set; } = string.Empty; // Soğutmalı Kıyma Makinesi (32'lik)
    public string EquipmentType { get; set; } = string.Empty; // Kıyma, Marinasyon Tamburu, Yoğurma Kazanı, vb.
    public string CapacityInfo { get; set; } = string.Empty;   // 600 kg/saat veya 50 kg/parti
    public string OperatingStatus { get; set; } = "Hazır / Operasyonel"; // Hazır, Çalışıyor, Bakımda, Dezenfekte Edildi
    
    public DateTime LastSanitizationDate { get; set; } = DateTime.UtcNow.AddHours(-3);
    public string SanitizedBy { get; set; } = "Mustafa Usta";
    public string SanitizingAgent { get; set; } = "Perasetik Asit (%0.2) + Sıcak Su (82°C)";
    public bool IsSanitizedAndReady { get; set; } = true;
    public DateTime? NextMaintenanceDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public string? Notes { get; set; }
}

public class AuxiliaryMaterialStock : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string MaterialCode { get; set; } = string.Empty; // BAH-SCK-01
    public string MaterialName { get; set; } = string.Empty; // Özel Sucuk Baharat Harcı
    public RecipeItemType ItemType { get; set; } = RecipeItemType.SpiceMix;
    
    public decimal CurrentStock { get; set; } // Mevcut Miktar (Kg / Adet / Metre)
    public string UnitOfMeasure { get; set; } = "KG"; // KG, ADT, MT
    public decimal UnitCost { get; set; } // Birim Maliyet
    public decimal MinStockLevel { get; set; } = 5.0m;
    public decimal TotalValue => Math.Round(CurrentStock * UnitCost, 2);
    
    public DateTime? ExpiryDate { get; set; }
    public string StorageCondition { get; set; } = "Kuru & Serin (+15°C)";
    public bool IsLowStock => CurrentStock <= MinStockLevel;
}

public class ReprocessingQualityCheck : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string InspectionNumber { get; set; } = string.Empty; // QC-2026-0913-001
    public string BatchLotNumber { get; set; } = string.Empty;   // LOT-2026-KYM-01
    public string ProductName { get; set; } = string.Empty;      // Dana Kıyma
    
    public decimal MeasuredPh { get; set; } = 5.60m; // İdeal: 5.4 - 5.8, Kritik Limit: > 6.2 (Bozulma/İmha)
    public decimal MeasuredWaterActivityAw { get; set; } = 0.88m; // İdeal sucuk için < 0.90
    public decimal MeasuredCoreTempCelsius { get; set; } = 2.4m; // İdeal et hazırlık: <= 4°C
    
    public string QualityVerdict { get; set; } = "Onaylandı (Güvenli Dönüşüm)"; // Onaylandı, Şartlı Uygun, ⛔ Reddedildi
    public bool IsApproved { get; set; } = true;
    public bool IsAutoBlocked { get; set; } = false; // pH > 6.2 ise true
    
    public string InspectorName { get; set; } = "Dr. Vet. Mehmet Demir (HACCP Denetçisi)";
    public string? Notes { get; set; }
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
}
