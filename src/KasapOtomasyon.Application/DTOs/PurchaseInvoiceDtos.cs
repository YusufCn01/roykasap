using KasapOtomasyon.Domain.Entities;

namespace KasapOtomasyon.Application.DTOs;

public class PurchaseInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? WaybillNumber { get; set; }
    public PurchaseInvoiceType InvoiceType { get; set; }
    public string InvoiceTypeDisplay => InvoiceType switch
    {
        PurchaseInvoiceType.TicariAlimFaturasi => "Ticari Alım (KDV'li)",
        PurchaseInvoiceType.MustahsilMakbuzu => "Müstahsil Makbuzu (Stopajlı)",
        PurchaseInvoiceType.IrsaliyeliAlim => "İrsaliyeli Alım",
        PurchaseInvoiceType.AlimIadesi => "Alım İadesi",
        _ => InvoiceType.ToString()
    };

    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierTaxNo { get; set; }
    public string? SupplierPhone { get; set; }

    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? EInvoiceUuid { get; set; }
    public string? EInvoiceStatus { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal VatTotal { get; set; }
    public decimal TevkifatTotal { get; set; }
    public decimal WithholdingTaxTotal { get; set; }
    public decimal SgkBagkurTotal { get; set; }
    public decimal BorsaFeeTotal { get; set; }
    public decimal MeraFundTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public PurchasePaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusDisplay => PaymentStatus switch
    {
        PurchasePaymentStatus.Odenmedi => "Ödenmedi",
        PurchasePaymentStatus.KismiOdendi => "Kısmi Ödendi",
        PurchasePaymentStatus.Odendi => "Ödendi",
        _ => PaymentStatus.ToString()
    };

    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool IsStockUpdated { get; set; }
    public string? Notes { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQuantityKg { get; set; }

    public List<PurchaseInvoiceLineDto> Lines { get; set; } = new();
}

public class PurchaseInvoiceLineDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public PurchaseItemType ItemType { get; set; }
    public string ItemTypeDisplay => ItemType switch
    {
        PurchaseItemType.KarkasEt => "Karkas Et",
        PurchaseItemType.KemiksizEt => "Kemiksiz Lop Et",
        PurchaseItemType.Sakatat => "Sakatat",
        PurchaseItemType.CanliHayvan => "Canlı Hayvan",
        PurchaseItemType.BaharatVeKatki => "Baharat & Katkı",
        PurchaseItemType.AmbalajVeSarf => "Ambalaj & Sarf",
        _ => ItemType.ToString()
    };

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "Kg";
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; } = 1.0m;
    public decimal VatAmount { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TevkifatRate { get; set; }
    public decimal TevkifatAmount { get; set; }
    public decimal LineTotal { get; set; }

    public string? LotNumber { get; set; }
    public string? EarTagNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string StorageLocation { get; set; } = "Soğuk Depo #1";
}

public class CreatePurchaseInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? WaybillNumber { get; set; }
    public PurchaseInvoiceType InvoiceType { get; set; } = PurchaseInvoiceType.TicariAlimFaturasi;
    public int SupplierId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public bool AutoUpdateStock { get; set; } = true;

    public List<CreatePurchaseInvoiceLineDto> Lines { get; set; } = new();
}

public class CreatePurchaseInvoiceLineDto
{
    public int? ProductId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public PurchaseItemType ItemType { get; set; } = PurchaseItemType.KarkasEt;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "Kg";
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; } = 1.0m;
    public decimal DiscountRate { get; set; }
    public decimal TevkifatRate { get; set; }
    public string? LotNumber { get; set; }
    public string? EarTagNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string StorageLocation { get; set; } = "Soğuk Depo #1";
}

public class PurchaseInvoiceFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? SupplierId { get; set; }
    public PurchaseInvoiceType? InvoiceType { get; set; }
    public PurchasePaymentStatus? PaymentStatus { get; set; }
    public string? SearchQuery { get; set; }
}

public class PurchaseKpiSummaryDto
{
    public decimal MonthlyPurchaseAmount { get; set; } // Bu ayki toplam alım tutarı ₺
    public decimal MonthlyPurchaseWeightKg { get; set; } // Bu ayki toplam alım tonajı kg
    public decimal TotalTevkifatDeducted { get; set; } // Toplam tevkifat kesintisi ₺
    public decimal TotalWithholdingTax { get; set; } // Toplam müstahsil stopajı ₺
    public decimal PendingSupplierPayables { get; set; } // Tedarikçilere ödenecek bekleyen borç ₺
    public int TotalInvoiceCount { get; set; }
}
