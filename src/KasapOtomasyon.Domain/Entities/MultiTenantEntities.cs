namespace KasapOtomasyon.Domain.Entities;

public interface ITenantScoped
{
    int TenantId { get; set; }
}

public interface ICompanyScoped : ITenantScoped
{
    int CompanyId { get; set; }
}

public interface IPlantScoped : ICompanyScoped
{
    int PlantId { get; set; }
}

public class Tenant : BaseEntity
{
    public string Code { get; set; } = "TNT-001";
    public string Name { get; set; } = "Global Meat Operations Group";
    public string HoldingName { get; set; } = "RoyPos International AgriFood Group";
    public string CountryCode { get; set; } = "TR";
    public string TaxNumber { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = "tr-TR"; // tr-TR, en-GB, pl-PL

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}

public class Company : BaseEntity, ITenantScoped
{
    public int TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string Code { get; set; } = "CMP-TR-01";
    public string Name { get; set; } = "RoyPos Et ve Mezbaha San. Tic. A.Ş.";
    public string CountryCode { get; set; } = "TR"; // TR, PL, GB, DE
    public string CurrencyCode { get; set; } = "TRY"; // TRY, EUR, PLN, GBP, USD
    public string DefaultLanguage { get; set; } = "tr-TR";
    public string TaxOffice { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}

public enum PlantType
{
    SlaughterhouseAndProcessing = 1, // Entegre Mezbaha ve Parçalama Tesisi
    SlaughterhouseOnly = 2,           // Kesimhane
    DeboningAndProcessing = 3,        // Parçalama ve İşleme Tesisi
    ColdStorageHub = 4,               // Lojistik Soğuk Hava Deposu
    ButcheryRetailChain = 5           // Kasap ve Perakende Zinciri
}

public class Plant : BaseEntity, ICompanyScoped
{
    public int TenantId { get; set; }
    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;

    public string Code { get; set; } = "PLN-IST-01";
    public string Name { get; set; } = "İstanbul Entegre Mezbaha & Et İşleme Tesisi";
    public PlantType PlantType { get; set; } = PlantType.SlaughterhouseAndProcessing;
    public string OfficialApprovalNumber { get; set; } = "TR-34-MEZ-0089"; // Bakanlık Tesis Onay No
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = "İstanbul";
    public string CountryCode { get; set; } = "TR";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PlantManagerName { get; set; } = "Dr. Vet. Mehmet Demir";
    public string Phone { get; set; } = string.Empty;
    public string IpSubnet { get; set; } = "192.168.1.0/24";

    public virtual ICollection<FacilityUnit> FacilityUnits { get; set; } = new List<FacilityUnit>();
}

public enum FacilityUnitType
{
    LairageReceiving = 1,    // Padok / Hayvan Kabul İstasyonu
    SlaughterLine = 2,       // Kesim Hattı
    CarcassGradingArea = 3,  // Karkas Tartım & SEUROP Sınıflandırma
    ChillingColdRoom = 4,    // Soğuk Hava Deposu
    DeboningRoom = 5,        // Parçalama & Kemiksizleştirme Salonu
    MeatProcessingRoom = 6,  // Şarküteri / Kıyma / Sucuk Üretim
    PackagingStation = 7,    // GS1 Paketleme & Etiketleme
    WmsWarehouse = 8,        // Donuk / Taze WMS Depo
    DispatchLoadingDock = 9, // Sevkiyat Yükleme Rampası
    RetailButcheryPos = 10   // Kasap Satış Reyonu
}

public class FacilityUnit : BaseEntity, IPlantScoped
{
    public int TenantId { get; set; }
    public int CompanyId { get; set; }
    public int PlantId { get; set; }
    public virtual Plant Plant { get; set; } = null!;

    public string Code { get; set; } = "UNT-DEB-01";
    public string Name { get; set; } = "Ana Parçalama & Kemiksizleştirme Salonu";
    public FacilityUnitType UnitType { get; set; } = FacilityUnitType.DeboningRoom;
    public decimal TargetTemperatureCelsius { get; set; } = 8.0m;
    public string AssignedSupervisor { get; set; } = "Usta Kasap Hüseyin Usta";
}
