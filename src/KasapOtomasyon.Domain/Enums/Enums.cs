namespace KasapOtomasyon.Domain.Enums;

public enum AnimalType
{
    Buyukbas = 1,
    Kucukbas = 2,
    Kanatli = 3,
    Balik = 4
}

public enum AnimalCarcassType
{
    Karkas = 1,
    CeyrekGovde = 2,
    CanliHayvan = 3,
    Parcalanmis = 4
}

public enum ProductType
{
    Tartili = 1, // Kg/Gr
    Adetli = 2
}

public enum PaymentType
{
    Nakit = 1,
    KrediKarti = 2,
    CariHesap = 3,
    HavaleEFT = 4,
    Parcali = 5
}

public enum SaleStatus
{
    Tamamlandi = 1,
    Beklemede = 2,
    Iptal = 3,
    Iade = 4
}

public enum LicenseType
{
    Deneme = 1,       // 15 Günlük Otomatik
    Standart = 2,     // Temel Kasap POS, Stok, Kasa
    Profesyonel = 3,  // Standart + Mezbaha BOM, Parçalama Randıman, Gelişmiş Raporlar, Terazi
    Kurumsal = 4      // Profesyonel + Çoklu Şube, E-Fatura, SMS, Bulut Yedekleme
}

public enum LicenseStatus
{
    Gecerli = 1,
    SuresiDoldu = 2,
    Gecersiz = 3,
    DonanimUyusmazligi = 4,
    LisansBulunamadi = 5
}

public enum StockMovementType
{
    Giris = 1,
    Cikis = 2,
    UretimCikis = 3,
    UretimGiris = 4,
    TransferGiris = 5,
    TransferCikis = 6,
    SayimFarki = 7,
    Satis = 8,
    Iade = 9,
    ReyonKurtarmaCikis = 10,
    ReyonKurtarmaGiris = 11
}

public enum StockCountMode
{
    Akilli = 1,
    Online = 2,
    Anlik = 3
}

public enum InvoiceType
{
    SatisFaturasi = 1,
    AlisFaturasi = 2,
    IadeFaturasi = 3,
    Irsaliye = 4
}

public enum EInvoiceStatus
{
    Taslak = 1,
    Kuyrukta = 2,
    Gonderildi = 3,
    Onaylandi = 4,
    Reddedildi = 5,
    Hata = 6
}

public enum UserRoleType
{
    // Executive & Management
    GroupCeo = 1,
    CountryManager = 2,
    PlantManager = 3,
    SystemAdministrator = 4,

    // Operations & Supervisors
    SlaughterSupervisor = 5,
    CuttingSupervisor = 6,
    ProductionManager = 7,
    WarehouseManager = 8,

    // Quality, Compliance & Health
    Veterinarian = 9,
    QualityManager = 10,
    Auditor = 11,

    // Commercial & Finance
    PurchasingManager = 12,
    SalesManager = 13,
    FinanceAccounting = 14,

    // Line Workers & Retail
    MasterButcher = 15,
    StationOperator = 16,
    PosCashier = 17,
    LogisticsDriver = 18,

    // Backward-compatibility aliases
    Yonetici = 4,
    MagazaMuduru = 3,
    Kasiyer = 17,
    KasapUretimSorumlusu = 15,
    Muhasebe = 14
}

public enum ProductionOrderStatus
{
    Planlandi = 1,
    DevamEdiyor = 2,
    Tamamlandi = 3,
    Iptal = 4
}

public enum SlaughterLineState
{
    Received = 1,               // Padokta Bekliyor
    WaitingAntemortem = 2,      // Veteriner Muayenesi Bekliyor
    ApprovedForSlaughter = 3,   // Kesim Onaylandı
    SlaughterScheduled = 4,     // Kesim Sırasına Alındı
    Slaughtering = 5,           // Kesim ve Kan Akıtma
    Slaughtered = 6,            // Kesildi (Deri/Baş Ayrıldı)
    CarcassCreated = 7,         // Karkas Oluşturuldu
    PostmortemCheck = 8,        // Postmortem Muayene & Karkas Damgalama
    Weighed = 9,                // Monoray Kantarında Tartıldı
    GradedSeurop = 10,          // SEUROP Derecelendirildi
    Chilling = 11,              // Soğuk Havada Dinlendiriliyor
    ReleasedForDeboning = 12    // Parçalamaya / Sevkiyata Çıkarıldı
}

public enum BarcodeType
{
    EAN13 = 1,
    Code128 = 2,
    EmbeddedWeight = 3 // 20 + PLU + Weight + Check
}

public enum VeterinaryCheckStatus
{
    Uygun = 1,
    SartliUygun = 2,
    KarantinaBloke = 3,
    Imha = 4
}

public enum SlaughterStatus
{
    Bekliyor = 1,
    Kesimde = 2,
    Kesildi = 3,
    VeterinerKontrolu = 4,
    KarkasTartildi = 5,
    ParcalamayaHazir = 6,
    Tamamlandi = 7
}

public enum AnimalGender
{
    Erkek = 1,
    Disi = 2,
    Tosun = 3,
    Duve = 4
}

public enum WasteCauseType
{
    KesimFiresi = 1,
    ParcalamaFiresi = 2,
    BozulmaSKT = 3,
    KemikYagFire = 4,
    FazlaTrim = 5,
    DusmeKirlenme = 6,
    PersonelHatasi = 7,
    Iade = 8,
    Diger = 9
}

public enum ColdStorageType
{
    SicakKarkasSoklama = 1,
    SogukHava = 2,
    DonukDepo = 3,
    ParcalamaReyonu = 4,
    SakatatDeposu = 5,
    Sevkiyat = 6
}

public enum AnomalyType
{
    KarkasStokKayip = 1,
    ManuelTartimDegisikligi = 2,
    YetkisizFiyatIndirimi = 3,
    NegatifStok = 4,
    MaliyetAltiSatis = 5,
    GeceIslemi = 6
}

