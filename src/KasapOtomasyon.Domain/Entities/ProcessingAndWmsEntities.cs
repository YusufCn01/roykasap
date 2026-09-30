namespace KasapOtomasyon.Domain.Entities;

public enum RecipeProductCategory
{
    MinceAndGroundMeat = 1, // Kıyma / Kuşbaşı
    FreshSausage = 2,       // Taze Kasap Sucuğu
    DryFermentedSausage = 3, // Fermente / Kangal Sucuk
    MeatballsAndPatties = 4, // Kasap Köfte / Burger
    MarinatedPrimalCuts = 5, // Marine Edilmiş Asil Etler
    PastramiAndSmoked = 6,  // Pastırma / Füme Et
    DeliAndCooked = 7       // Kavurma / Söğüş
}

public class ProcessingRecipe : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string RecipeCode { get; set; } = "RCP-SUCUK-01";
    public string RecipeName { get; set; } = "Geleneksel Fermente Kasap Sucuğu Reçetesi";
    public RecipeProductCategory Category { get; set; } = RecipeProductCategory.DryFermentedSausage;
    
    public int? TargetProductId { get; set; }
    public string TargetProductName { get; set; } = "Kasap Sucuğu";

    public decimal StandardBatchWeightKg { get; set; } = 100.0m;
    public decimal ExpectedYieldPercentage { get; set; } = 92.0m; // Kuruma sonrası randıman
    public int ShelfLifeExtensionDays { get; set; } = 45; // Dönüşüm sonrası ilave raf ömrü (Gün)
    
    public string Description { get; set; } = "Doğal bağırsakta, özel baharat karışımlı fermente sucuk";
    public string ProcessInstructions { get; set; } = "Et ve yağ 3 mm aynadan çekilir. Baharat karışımı ve tuz homojen yoğrulur. 24 saat +4°C'de dinlendirildikten sonra kurutma odasına alınır.";

    public virtual ICollection<ProcessingRecipeItem> RecipeItems { get; set; } = new List<ProcessingRecipeItem>();
}

public enum RecipeItemType
{
    MeatRawMaterial = 1,   // Ana Et Hammaddesi (Örn: Dana Döş %70)
    FatRawMaterial = 2,    // Yağ (Örn: Kavram / Kuyruk Yağı %20)
    SpiceMix = 3,          // Baharat Karışımı (Kimyon, Sarımsak, Kırmızı Biber)
    CasingAndClip = 4,     // Doğal Bağırsak / Klips
    PackagingMaterial = 5, // Vakum Poşeti / Koli
    SauceAndMarinade = 6,  // Sos & Sıvı Marinasyon (Barbekü, Hardal, Köri, Zeytinyağı)
    SaltAndCuring = 7      // Tuz & Kürleme / Koruyucu Karışımı
}

public class ProcessingRecipeItem : BaseEntity
{
    public int ProcessingRecipeId { get; set; }
    public virtual ProcessingRecipe ProcessingRecipe { get; set; } = null!;

    public RecipeItemType ItemType { get; set; } = RecipeItemType.MeatRawMaterial;
    public string ItemName { get; set; } = "Dana Döş / Tranç Et";
    public decimal PercentageRatio { get; set; } = 75.0m; // %75
    public decimal QuantityPerBatchKg { get; set; } = 75.0m;
    public decimal StandardUnitCost { get; set; } = 380.0m;
    public string Description { get; set; } = string.Empty; // Bileşen açıklaması ve lezzet/koruyuculuk etkisi
}

public class PalletSSCC : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; } = 1;
    public int CompanyId { get; set; } = 1;
    public int PlantId { get; set; } = 1;

    public string Sscc18Barcode { get; set; } = "00386901234567890128"; // (00) Serial Shipping Container Code
    public string PalletNumber { get; set; } = "PAL-2026-00045";
    public string LotNumber { get; set; } = "LOT-2026-0823-01";
    public string ProductName { get; set; } = "Vakumlu Dana Antrikot (1. Sınıf)";
    
    public int TotalBoxesCount { get; set; } = 40;
    public decimal TotalNetWeightKg { get; set; } = 480.50m;
    public decimal TotalGrossWeightKg { get; set; } = 512.00m;
    public string TargetWarehouseBin { get; set; } = "A-02-04-01";
    public string? DestinationCustomer { get; set; }
    public bool IsAllocatedForShipment { get; set; } = false;
    public bool IsQuarantined { get; set; } = false;
}
