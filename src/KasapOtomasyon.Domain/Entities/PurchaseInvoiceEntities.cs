using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public enum PurchaseInvoiceType
{
    TicariAlimFaturasi = 1, // KDV'li Toptan Et / Karkas / Malzeme Alımı
    MustahsilMakbuzu = 2,   // Çiftçi / Besiciden Stopajlı Canlı Hayvan Alımı
    IrsaliyeliAlim = 3,     // Sevk İrsaliyesi ile Alım
    AlimIadesi = 4          // Tedarikçiye İade Faturası
}

public enum PurchaseItemType
{
    KarkasEt = 1,       // Dana / Kuzu Karkas
    KemiksizEt = 2,     // Lop Et, Bonfile, Antrikot
    Sakatat = 3,        // Ciğer, İşkembe, Dil, Kelle
    CanliHayvan = 4,    // Canlı Besi Danası / Kuzu
    BaharatVeKatki = 5, // Sucuk Harcı, Baharatlar, Bağırsak
    AmbalajVeSarf = 6   // Vakum Torbası, Koli, Streç
}

public enum PurchasePaymentStatus
{
    Odenmedi = 1,
    KismiOdendi = 2,
    Odendi = 3
}

public class PurchaseInvoice : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string InvoiceNumber { get; set; } = string.Empty; // Örn: ALM-2026-000841 veya GIB2026000000841
    public string? WaybillNumber { get; set; } // Sevk İrsaliyesi No
    public PurchaseInvoiceType InvoiceType { get; set; } = PurchaseInvoiceType.TicariAlimFaturasi;

    public int SupplierId { get; set; } // Tedarikçi / Besici Müşteri Kartı
    public virtual Customer Supplier { get; set; } = null!;

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveryDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; } // Vade Tarihi

    // GİB E-Fatura / E-Müstahsil Bilgileri
    public string? EInvoiceUuid { get; set; }
    public string? EInvoiceStatus { get; set; } = "Yerel Kayıt"; // Yerel Kayıt, GİB'e İletildi, Onaylandı

    // Financial Breakdown
    public decimal SubTotal { get; set; }          // KDV Hariç Ara Toplam
    public decimal DiscountTotal { get; set; }      // Satır / Genel İskonto
    public decimal VatTotal { get; set; }           // Toplam KDV Tutarı
    
    // Et Sektörü Özel Vergi ve Kesintileri
    public decimal TevkifatTotal { get; set; }      // Tevkifat Tutarı (Örn: 9/10 veya 4/10 Et Tevkifatı)
    public decimal WithholdingTaxTotal { get; set; }// Müstahsil Gelir Vergisi Stopajı (%1 Canlı Hayvan, %2 Karkas)
    public decimal SgkBagkurTotal { get; set; }     // Müstahsil SGK Bağ-Kur Kesintisi (%2)
    public decimal BorsaFeeTotal { get; set; }      // Ticaret Borsası Tescil Ücreti (%0.1 - %0.2)
    public decimal MeraFundTotal { get; set; }      // Mera Fonu Kesintisi (%0.1)
    
    public decimal GrandTotal { get; set; }         // Net Ödenecek / Cari Borçlanılan Tutar

    // Payment Tracking
    public PurchasePaymentStatus PaymentStatus { get; set; } = PurchasePaymentStatus.Odenmedi;
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => GrandTotal - PaidAmount;

    public bool IsStockUpdated { get; set; } = true; // Ürünler stoklara işlendi mi?
    public string? Notes { get; set; }
    public string CreatedByUsername { get; set; } = "admin";

    public virtual ICollection<PurchaseInvoiceLine> Lines { get; set; } = new List<PurchaseInvoiceLine>();
}

public class PurchaseInvoiceLine : BaseEntity
{
    public int PurchaseInvoiceId { get; set; }
    public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;

    public int? ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public PurchaseItemType ItemType { get; set; } = PurchaseItemType.KarkasEt;

    public decimal Quantity { get; set; } // Miktar (Kg / Adet)
    public string Unit { get; set; } = "Kg";
    public decimal UnitPrice { get; set; } // KDV Hariç Birim Alış Fiyatı

    public decimal VatRate { get; set; } = 1.0m; // %1 (Toptan Et/Canlı Hayvan), %10, %20
    public decimal VatAmount { get; set; }

    public decimal DiscountRate { get; set; } // % İskonto
    public decimal DiscountAmount { get; set; }

    public decimal TevkifatRate { get; set; } = 0m; // 0.90 for 9/10 Tevkifat, 0 if None
    public decimal TevkifatAmount { get; set; }

    public decimal LineTotal { get; set; } // Satır Toplam Tutarı (BirimFiyat * Miktar - İskonto + KDV - Tevkifat)

    // Traceability & Inventory Storage
    public string? LotNumber { get; set; } // Parti / Lot No (Örn: LOT-2026-0814)
    public string? EarTagNumber { get; set; } // Canlı Hayvan ise Küpe No (Örn: TR340019283)
    public DateTime? ExpiryDate { get; set; }
    public string StorageLocation { get; set; } = "Soğuk Depo #1";
}
