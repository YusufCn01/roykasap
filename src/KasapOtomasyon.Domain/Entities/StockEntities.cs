using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Ana Soğuk Hava Deposu, Tezgâh Reyon Deposu, Mezbaha Deposu
    public string Code { get; set; } = string.Empty; // DEP-01
    public string? Location { get; set; }
    public bool IsDefault { get; set; } = true;

    public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}

public class StockItem : BaseEntity
{
    public int WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public decimal CurrentQuantity { get; set; } // Mevcut Miktar (Kg / Adet)
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => Math.Max(0, CurrentQuantity - ReservedQuantity);
    
    public string? LotNumber { get; set; } // Parti / Karkas Lot No
    public DateTime? ExpiryDate { get; set; } // Genel SKT - Son Kullanma Tarihi
    public DateTime? ShelfDisplayExpiryDate { get; set; } // Reyon Son Kullanım Tarihi (Kullanıcı tarafından girilebilir/güncellenebilir)
    public string? ShelfLocation { get; set; } // Tezgâh / Reyon Vitrin Rafı (örn. "REYON-A1-03")
    public string? ReprocessingNotes { get; set; } // Kurtarma & Dönüşüm Notları

    public DateTime ReyonExpiryDate => ShelfDisplayExpiryDate ?? ExpiryDate ?? DateTime.MaxValue;
    public double ReyonDaysRemaining => (ShelfDisplayExpiryDate.HasValue || ExpiryDate.HasValue) 
        ? Math.Round((ReyonExpiryDate - DateTime.UtcNow).TotalDays, 1) 
        : 999;
    public bool IsCriticalExpiry => ExpiryDate.HasValue && (ExpiryDate.Value - DateTime.UtcNow).TotalDays <= 2;
    public bool IsReyonCritical => ReyonDaysRemaining <= 2;
}

public class StockMovement : BaseEntity
{
    public int WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public StockMovementType MovementType { get; set; } = StockMovementType.Giris;
    public decimal Quantity { get; set; } // Girişler pozitif, çıkışlar negatif
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount => Math.Round(Quantity * UnitPrice, 2);
    
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ReferenceNumber { get; set; } // İrsaliye No, Fiş No, Üretim Emri No
    public string? Description { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
}

public class StockCount : BaseEntity
{
    public string CountNumber { get; set; } = string.Empty; // SYM-2026-001
    public int WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;

    public DateTime CountDate { get; set; } = DateTime.UtcNow;
    public StockCountMode CountMode { get; set; } = StockCountMode.Akilli;
    public string Status { get; set; } = "Tamamlandi"; // "Taslak", "Tamamlandi", "Iptal"
    public string ConductedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public virtual ICollection<StockCountItem> Items { get; set; } = new List<StockCountItem>();
}

public class StockCountItem : BaseEntity
{
    public int StockCountId { get; set; }
    public virtual StockCount StockCount { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal DifferenceQuantity => CountedQuantity - SystemQuantity;
    public decimal UnitCost { get; set; }
    public decimal DifferenceCost => Math.Round(DifferenceQuantity * UnitCost, 2);
    public string? Notes { get; set; }
}

public class WarehouseTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty; // TRF-2026-001
    
    public int SourceWarehouseId { get; set; }
    public virtual Warehouse SourceWarehouse { get; set; } = null!;

    public int DestinationWarehouseId { get; set; }
    public virtual Warehouse DestinationWarehouse { get; set; } = null!;

    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Onaylandi"; // "Beklemede", "Onaylandi", "Reddedildi"
    public string RequestedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? Notes { get; set; }

    public virtual ICollection<WarehouseTransferItem> Items { get; set; } = new List<WarehouseTransferItem>();
}

public class WarehouseTransferItem : BaseEntity
{
    public int WarehouseTransferId { get; set; }
    public virtual WarehouseTransfer WarehouseTransfer { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public decimal Quantity { get; set; }
    public string? LotNumber { get; set; }
}
