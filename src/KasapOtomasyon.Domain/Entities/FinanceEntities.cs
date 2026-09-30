using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class CashRegister : BaseEntity
{
    public string Name { get; set; } = "Ana Kasa";
    public string Code { get; set; } = "KAS-01";
    public int? WarehouseId { get; set; }
    public decimal CurrentBalance { get; set; } // Mevcut Nakit Bakiye

    public virtual ICollection<CashSession> Sessions { get; set; } = new List<CashSession>();
}

public class CashSession : BaseEntity
{
    public int CashRegisterId { get; set; }
    public virtual CashRegister CashRegister { get; set; } = null!;

    public int? UserId { get; set; }
    public string CashierName { get; set; } = "Kasiyer";

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    
    public decimal OpeningBalance { get; set; } // Açılış Devir Nakit
    public decimal TotalSalesCash { get; set; } // Nakit Satışlar
    public decimal TotalSalesCreditCard { get; set; } // Kredi Kartı Satışlar
    public decimal TotalSalesOnAccount { get; set; } // Veresiye / Cari Satışlar
    public decimal TotalCashIn { get; set; } // Diğer Girişler
    public decimal TotalCashOut { get; set; } // Masraf / Çıkışlar
    
    public decimal ClosingCalculatedBalance => OpeningBalance + TotalSalesCash + TotalCashIn - TotalCashOut;
    public decimal? ClosingActualBalance { get; set; } // Kasa Sayımında Çıkan
    public decimal? DiscrepancyAmount => ClosingActualBalance.HasValue ? ClosingActualBalance.Value - ClosingCalculatedBalance : null; // Kasa Farkı (+ Fazla, - Açık)
    
    public string Status { get; set; } = "Acik"; // "Acik", "Kapandi"
    public string? Notes { get; set; }

    public virtual ICollection<CashTransaction> Transactions { get; set; } = new List<CashTransaction>();
}

public class CashTransaction : BaseEntity
{
    public int CashSessionId { get; set; }
    public virtual CashSession CashSession { get; set; } = null!;

    public string TransactionType { get; set; } = "SatisTahsilat"; // "SatisTahsilat", "KasaGiris", "KasaCikis", "Masraf", "CariTahsilat", "CariOdeme"
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime TransactionTime { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
}

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty; // FAT-2026-00001
    public string Uuid { get; set; } = Guid.NewGuid().ToString();
    public InvoiceType InvoiceType { get; set; } = InvoiceType.SatisFaturasi;
    public EInvoiceStatus EInvoiceStatus { get; set; } = EInvoiceStatus.Taslak;

    public int? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    public decimal SubTotal { get; set; }
    public decimal VatTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1.0m;

    public string? Notes { get; set; }
    public string? GibStatusCode { get; set; }
    public string? GibStatusDescription { get; set; }

    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}

public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public virtual Invoice Invoice { get; set; } = null!;

    public int? ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitName { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; } = 1.0m;
    public decimal VatAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal => Math.Round((Quantity * UnitPrice) - DiscountAmount + VatAmount, 2);
}
