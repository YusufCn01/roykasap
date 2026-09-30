using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public virtual Category? ParentCategory { get; set; }
    public string? ColorCode { get; set; } // Hex e.g. #9E1B32
    public string? IconName { get; set; }
    public int DisplayOrder { get; set; }

    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class UnitOfMeasure : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Kilogram, Gram, Adet, Paket
    public string Code { get; set; } = string.Empty; // KG, GR, ADT, PKT
    public bool IsDecimalAllowed { get; set; } = true;
    public decimal ConversionFactor { get; set; } = 1.0m;
}

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g. PRD-001
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    public int UnitOfMeasureId { get; set; }
    public virtual UnitOfMeasure UnitOfMeasure { get; set; } = null!;

    public ProductType ProductType { get; set; } = ProductType.Tartili; // Tartılı / Adetli
    public decimal VatRate { get; set; } = 1.0m; // KDV %1 (Et toptan/perakende) veya %10
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    
    public decimal MinStockLevel { get; set; }
    public decimal MaxStockLevel { get; set; }
    public int ShelfLifeDays { get; set; } = 5; // SKT Gün Sayısı (Taze ette hayati önem taşır)
    
    public bool IsQuickButton { get; set; } = true; // POS Hızlı Satış Butonlarında göster
    public string? ButtonColor { get; set; } // Özel buton rengi
    public int DisplayOrder { get; set; }
    public string? PluCode { get; set; } // Terazi PLU Kodu (örn. 00123)

    public virtual ICollection<BarcodeDefinition> Barcodes { get; set; } = new List<BarcodeDefinition>();
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
    public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

public class BarcodeDefinition : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    public string Barcode { get; set; } = string.Empty;
    public BarcodeType BarcodeType { get; set; } = BarcodeType.EAN13;
    public bool IsDefault { get; set; } = true;
}

public class PriceList : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Perakende, Toptan, Bayi, Kampanya
    public string Code { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public bool IsDefault { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
}

public class ProductPrice : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    public int PriceListId { get; set; }
    public virtual PriceList PriceList { get; set; } = null!;
    public decimal Price { get; set; }
    public bool VatIncluded { get; set; } = true;
}
