using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class Sale : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty; // FIS-20260823-0001
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    
    public int? CashierId { get; set; }
    public string CashierName { get; set; } = "Kasiyer";

    public int? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public int? PriceListId { get; set; }
    public virtual PriceList? PriceList { get; set; }

    public decimal SubTotal { get; set; } // Ara Toplam
    public decimal DiscountTotal { get; set; } // İskonto Toplamı
    public decimal VatTotal { get; set; } // KDV Toplamı
    public decimal GrandTotal { get; set; } // Genel Toplam
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; } // Para Üstü

    public PaymentType PrimaryPaymentType { get; set; } = PaymentType.Nakit;
    public SaleStatus Status { get; set; } = SaleStatus.Tamamlandi;
    
    public bool IsParked { get; set; } // Satış Bekletmede mi?
    public string? ParkNote { get; set; }
    public string? TerminalId { get; set; }
    public string? Notes { get; set; }

    public virtual ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public virtual ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
}

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public virtual Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Quantity { get; set; } // Kg veya Adet
    public string UnitName { get; set; } = "KG";
    
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; } = 1.0m;
    public decimal VatAmount { get; set; }
    
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; } // (Quantity * UnitPrice) - DiscountAmount

    public string? TraceabilityLotNo { get; set; } // Etiket üzerindeki parti/lot no
}

public class SalePayment : BaseEntity
{
    public int SaleId { get; set; }
    public virtual Sale Sale { get; set; } = null!;

    public PaymentType PaymentType { get; set; } = PaymentType.Nakit;
    public decimal Amount { get; set; }
    
    public string? BankOrCreditCardName { get; set; }
    public int InstallmentCount { get; set; } = 1;
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public string? TransactionReference { get; set; }
}
