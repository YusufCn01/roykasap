using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(KasapDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await EnsureMezbahaTablesAndBreedsAsync(context);

        // 0. SEED ENTERPRISE MULTI-TENANT HIERARCHY IF NOT PRESENT
        var defaultTenant = await context.Tenants.FirstOrDefaultAsync();
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant
            {
                Code = "TNT-GLOBAL-01",
                Name = "Global Meat Operations Group",
                HoldingName = "RoyPos AgriFood International Holding",
                CountryCode = "TR",
                TaxNumber = "3400998877",
                ContactEmail = "corporate@roypos.global",
                ContactPhone = "+90 212 555 0000",
                DefaultLanguage = "tr-TR"
            };
            await context.Tenants.AddAsync(defaultTenant);
            await context.SaveChangesAsync();

            var trCompany = new Company
            {
                TenantId = defaultTenant.Id,
                Code = "CMP-TR-01",
                Name = "RoyPos Et ve Mezbaha San. Tic. A.Ş.",
                CountryCode = "TR",
                CurrencyCode = "TRY",
                DefaultLanguage = "tr-TR",
                TaxOffice = "Büyük Mükellefler",
                TaxNumber = "9988776655",
                Address = "İstanbul, Türkiye"
            };

            var plCompany = new Company
            {
                TenantId = defaultTenant.Id,
                Code = "CMP-PL-01",
                Name = "RoyPos Polska Zakłady Mięsne Sp. z o.o.",
                CountryCode = "PL",
                CurrencyCode = "PLN",
                DefaultLanguage = "pl-PL",
                TaxOffice = "Warszawa-Śródmieście",
                TaxNumber = "PL5252800000",
                Address = "Warszawa, Polska"
            };

            var ukCompany = new Company
            {
                TenantId = defaultTenant.Id,
                Code = "CMP-UK-01",
                Name = "RoyPos Global Meat Ltd.",
                CountryCode = "GB",
                CurrencyCode = "GBP",
                DefaultLanguage = "en-GB",
                TaxOffice = "HMRC London",
                TaxNumber = "GB999888777",
                Address = "London, United Kingdom"
            };

            await context.Companies.AddRangeAsync(trCompany, plCompany, ukCompany);
            await context.SaveChangesAsync();

            var plant1 = new Plant
            {
                TenantId = defaultTenant.Id,
                CompanyId = trCompany.Id,
                Code = "PLN-IST-01",
                Name = "İstanbul Entegre Mezbaha & Et İşleme Tesisi",
                PlantType = PlantType.SlaughterhouseAndProcessing,
                OfficialApprovalNumber = "TR-34-MEZ-0089",
                City = "İstanbul",
                CountryCode = "TR",
                PlantManagerName = "Dr. Vet. Mehmet Demir",
                Phone = "+90 212 444 0101"
            };

            var plant2 = new Plant
            {
                TenantId = defaultTenant.Id,
                CompanyId = plCompany.Id,
                Code = "PLN-WAR-01",
                Name = "Warszawa Zakład Uboju i Rozbioru Mięsa",
                PlantType = PlantType.SlaughterhouseAndProcessing,
                OfficialApprovalNumber = "PL-14650001-WET",
                City = "Warszawa",
                CountryCode = "PL",
                PlantManagerName = "Lek. Wet. Jan Kowalski",
                Phone = "+48 22 123 4567"
            };

            await context.Plants.AddRangeAsync(plant1, plant2);
            await context.SaveChangesAsync();

            var units = new List<FacilityUnit>
            {
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-01", Name = "Canlı Hayvan Kabul & Padok", UnitType = FacilityUnitType.LairageReceiving, TargetTemperatureCelsius = 18m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-02", Name = "Otomasyonlu Kesim Hattı (Büyükbaş/Küçükbaş)", UnitType = FacilityUnitType.SlaughterLine, TargetTemperatureCelsius = 14m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-03", Name = "Karkas SEUROP & Tartım İstasyonu", UnitType = FacilityUnitType.CarcassGradingArea, TargetTemperatureCelsius = 10m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-04", Name = "Monoray Soğuk Hava Depoları (SH-01..SH-04)", UnitType = FacilityUnitType.ChillingColdRoom, TargetTemperatureCelsius = 2m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-05", Name = "Karkas Parçalama & Kemiksizleştirme Salonu", UnitType = FacilityUnitType.DeboningRoom, TargetTemperatureCelsius = 8m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-06", Name = "Şarküteri, Kıyma & Sucuk İşleme", UnitType = FacilityUnitType.MeatProcessingRoom, TargetTemperatureCelsius = 6m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-07", Name = "GS1-128 Otomatik Paketleme & Etiketleme", UnitType = FacilityUnitType.PackagingStation, TargetTemperatureCelsius = 10m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-08", Name = "WMS Lojistik Sevkiyat Rampası", UnitType = FacilityUnitType.DispatchLoadingDock, TargetTemperatureCelsius = 4m },
                new() { TenantId = defaultTenant.Id, CompanyId = trCompany.Id, PlantId = plant1.Id, Code = "UNT-TR-09", Name = "RoyPos Kasap & Gurme Satış Reyonu", UnitType = FacilityUnitType.RetailButcheryPos, TargetTemperatureCelsius = 16m }
            };
            await context.FacilityUnits.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }

        if (await context.Users.AnyAsync())
        {
            return; // DB core users have already been seeded
        }

        // 1. SEED 18 ENTERPRISE ROLES & PERMISSIONS
        var roles = new List<Role>
        {
            new() { Name = "Group CEO", Description = "Global Holding & Tüm Ülkeler/Tesisler Tam Yetki", RoleType = UserRoleType.GroupCeo },
            new() { Name = "Country Manager", Description = "Ülke Genel Müdürü ve Mevzuat Sorumlusu", RoleType = UserRoleType.CountryManager },
            new() { Name = "Tesis Müdürü (Plant Manager)", Description = "Tesis, Kesimhane, Parçalama ve WMS Genel Yetkisi", RoleType = UserRoleType.PlantManager },
            new() { Name = "Sistem Yöneticisi (System Admin)", Description = "Tüm Sistem Ayarları, Rol ve Güvenlik Yönetimi", RoleType = UserRoleType.SystemAdministrator },
            new() { Name = "Resmi Veteriner Hekim", Description = "Ante-mortem & Post-mortem Muayene ve Kesim İzin Yetkisi", RoleType = UserRoleType.Veterinarian },
            new() { Name = "Kalite Güvence & HACCP Müdürü", Description = "HACCP CCP İzleme, Karantina ve Geri Çağırma Yetkisi", RoleType = UserRoleType.QualityManager },
            new() { Name = "Kesimhane Sorumlusu (Supervisor)", Description = "Kesim Hattı MES, Canlı/Monoray Kantar ve SEUROP", RoleType = UserRoleType.SlaughterSupervisor },
            new() { Name = "Parçalama Şefi (Cutting Supervisor)", Description = "Karkas Parçalama MES, Kütle Dengesi ve Randıman", RoleType = UserRoleType.CuttingSupervisor },
            new() { Name = "Üretim Müdürü (Processing Manager)", Description = "Şarküteri, Kıyma, Reçete/BOM ve Parti Üretim", RoleType = UserRoleType.ProductionManager },
            new() { Name = "Depo & WMS Müdürü", Description = "Soğuk Depo, Raf, Palet, SSCC-18 ve Stok Hareketleri", RoleType = UserRoleType.WarehouseManager },
            new() { Name = "Satın Alma Müdürü", Description = "Hayvan Alımı, Çiftlik/Tedarikçi Yönetimi ve Siparişler", RoleType = UserRoleType.PurchasingManager },
            new() { Name = "Satış & İhracat Müdürü", Description = "B2B Satış, Müşteri Sözleşmeleri ve İhracat Evrakları", RoleType = UserRoleType.SalesManager },
            new() { Name = "Finans & Maliyet Muhasebesi", Description = "ABC Karkas Maliyet Dağılımı, Cari, Kasa ve E-Fatura", RoleType = UserRoleType.FinanceAccounting },
            new() { Name = "Usta Kasap", Description = "Karkas Kesim, Parçalama ve Tezgah İşlemleri", RoleType = UserRoleType.MasterButcher },
            new() { Name = "İstasyon Operatörü", Description = "Kantar, RFID ve GS1-128 Etiket Basım Kioskları", RoleType = UserRoleType.StationOperator },
            new() { Name = "POS Kasiyeri", Description = "Dokunmatik Hızlı Kasap Satış ve Kasa İşlemleri", RoleType = UserRoleType.PosCashier },
            new() { Name = "Soğuk Zincir Lojistik Sürücüsü", Description = "Araç Yükleme, Sıcaklık Takibi ve Teslimat Kanıtı", RoleType = UserRoleType.LogisticsDriver },
            new() { Name = "Denetçi & Müfettiş (Auditor)", Description = "Değişmez Denetim İzi (Audit) ve Anti-Fraud İnceleme", RoleType = UserRoleType.Auditor }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        var adminRole = roles.First(r => r.RoleType == UserRoleType.SystemAdministrator);
        var cashierRole = roles.First(r => r.RoleType == UserRoleType.PosCashier);
        var butcherRole = roles.First(r => r.RoleType == UserRoleType.MasterButcher);
        var vetRole = roles.First(r => r.RoleType == UserRoleType.Veterinarian);

        // 2. DEFAULT USERS
        // Password for admin: "admin123", PIN: "1234"
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        var pinHash = BCrypt.Net.BCrypt.HashPassword("1234");

        var adminUser = new User
        {
            Username = "admin",
            FullName = "Sistem Yöneticisi",
            PasswordHash = passwordHash,
            PinCodeHash = pinHash,
            RoleId = adminRole.Id,
            Email = "admin@kasapotomasyon.com",
            PhoneNumber = "05551234567",
            IsActive = true
        };

        var cashierUser = new User
        {
            Username = "kasiyer1",
            FullName = "Ahmet Yılmaz (Kasa 1)",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("kasa123"),
            PinCodeHash = BCrypt.Net.BCrypt.HashPassword("0000"),
            RoleId = cashierRole.Id,
            PhoneNumber = "05559876543",
            IsActive = true
        };

        var butcherUser = new User
        {
            Username = "kasap1",
            FullName = "Mustafa Usta (Baş Kasap)",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("kasap123"),
            PinCodeHash = BCrypt.Net.BCrypt.HashPassword("1111"),
            RoleId = butcherRole.Id,
            PhoneNumber = "05554443322",
            IsActive = true
        };

        await context.Users.AddRangeAsync(adminUser, cashierUser, butcherUser);

        // 3. UNITS OF MEASURE
        var unitKg = new UnitOfMeasure { Name = "Kilogram", Code = "KG", IsDecimalAllowed = true, ConversionFactor = 1.0m };
        var unitGr = new UnitOfMeasure { Name = "Gram", Code = "GR", IsDecimalAllowed = true, ConversionFactor = 0.001m };
        var unitAdet = new UnitOfMeasure { Name = "Adet", Code = "ADT", IsDecimalAllowed = false, ConversionFactor = 1.0m };
        var unitPkt = new UnitOfMeasure { Name = "Paket", Code = "PKT", IsDecimalAllowed = false, ConversionFactor = 1.0m };

        await context.UnitsOfMeasure.AddRangeAsync(unitKg, unitGr, unitAdet, unitPkt);

        // 4. CATEGORIES
        var catDana = new Category { Name = "Dana Eti", Description = "Büyükbaş Taze Et Çeşitleri", ColorCode = "#9E1B32", DisplayOrder = 1 };
        var catKuzu = new Category { Name = "Kuzu Eti", Description = "Küçükbaş Taze Et Çeşitleri", ColorCode = "#C41E3A", DisplayOrder = 2 };
        var catKiymaKofte = new Category { Name = "Kıyma & Köfte", Description = "Taze Çekim Kıyma ve Hazır Köfteler", ColorCode = "#800020", DisplayOrder = 3 };
        var catSarkuteri = new Category { Name = "Şarküteri & İşlenmiş", Description = "Sucuk, Pastırma, Kavurma", ColorCode = "#B22222", DisplayOrder = 4 };
        var catSakatat = new Category { Name = "Sakatat & Yan Ürün", Description = "Ciğer, Yürek, İşkembe vb.", ColorCode = "#A52A2A", DisplayOrder = 5 };
        var catTavuk = new Category { Name = "Kanatlı & Tavuk", Description = "Taze Tavuk ve Hindi", ColorCode = "#D2691E", DisplayOrder = 6 };

        await context.Categories.AddRangeAsync(catDana, catKuzu, catKiymaKofte, catSarkuteri, catSakatat, catTavuk);
        await context.SaveChangesAsync();

        // 5. WAREHOUSES
        var mainWarehouse = new Warehouse { Name = "Ana Soğuk Hava Deposu", Code = "DEP-01", Location = "Bodrum Kat - Soğuk Hava -18C / +4C", IsDefault = true };
        var posStoreWarehouse = new Warehouse { Name = "Mağaza Tezgâh Reyon", Code = "DEP-02", Location = "Giriş Kat Satış Reyonu", IsDefault = false };
        var slaughterWarehouse = new Warehouse { Name = "Mezbaha Kesimhane Deposu", Code = "DEP-03", Location = "Kesimhane Kabul Alanı", IsDefault = false };

        await context.Warehouses.AddRangeAsync(mainWarehouse, posStoreWarehouse, slaughterWarehouse);

        // 6. PRICE LISTS
        var defaultPriceList = new PriceList { Name = "Standart Perakende Fiyatı", Code = "PERAKENDE", CurrencyCode = "TRY", IsDefault = true };
        var wholesalePriceList = new PriceList { Name = "Toptan / Restoran Fiyatı", Code = "TOPTAN", CurrencyCode = "TRY", IsDefault = false };
        await context.PriceLists.AddRangeAsync(defaultPriceList, wholesalePriceList);
        await context.SaveChangesAsync();

        // 7. DEFAULT PRODUCTS (Kasap ve Mezbaha Ürünleri)
        var products = new List<Product>
        {
            // DANA
            new Product
            {
                Code = "DAN-001",
                Name = "Dana Antrikot",
                CategoryId = catDana.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 680.00m,
                CostPrice = 480.00m,
                MinStockLevel = 10m,
                MaxStockLevel = 100m,
                ShelfLifeDays = 7,
                IsQuickButton = true,
                ButtonColor = "#9E1B32",
                DisplayOrder = 1,
                PluCode = "00001",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000100000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "DAN-002",
                Name = "Dana Bonfile",
                CategoryId = catDana.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 850.00m,
                CostPrice = 600.00m,
                MinStockLevel = 5m,
                MaxStockLevel = 50m,
                ShelfLifeDays = 7,
                IsQuickButton = true,
                ButtonColor = "#800020",
                DisplayOrder = 2,
                PluCode = "00002",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000200000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "DAN-003",
                Name = "Dana Kuşbaşı (Az Yağlı)",
                CategoryId = catDana.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 520.00m,
                CostPrice = 380.00m,
                MinStockLevel = 20m,
                MaxStockLevel = 250m,
                ShelfLifeDays = 5,
                IsQuickButton = true,
                ButtonColor = "#B22222",
                DisplayOrder = 3,
                PluCode = "00003",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000300000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "DAN-004",
                Name = "Dana Kıyma (Çift Çekim)",
                CategoryId = catKiymaKofte.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 480.00m,
                CostPrice = 350.00m,
                MinStockLevel = 30m,
                MaxStockLevel = 300m,
                ShelfLifeDays = 3,
                IsQuickButton = true,
                ButtonColor = "#A52A2A",
                DisplayOrder = 4,
                PluCode = "00004",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000400000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "DAN-005",
                Name = "Kasap Köfte (Özel Baharatlı)",
                CategoryId = catKiymaKofte.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 510.00m,
                CostPrice = 360.00m,
                MinStockLevel = 15m,
                MaxStockLevel = 150m,
                ShelfLifeDays = 4,
                IsQuickButton = true,
                ButtonColor = "#8B0000",
                DisplayOrder = 5,
                PluCode = "00005",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000500000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },

            // KUZU
            new Product
            {
                Code = "KUZ-001",
                Name = "Kuzu Pirzola",
                CategoryId = catKuzu.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 720.00m,
                CostPrice = 520.00m,
                MinStockLevel = 8m,
                MaxStockLevel = 80m,
                ShelfLifeDays = 6,
                IsQuickButton = true,
                ButtonColor = "#C41E3A",
                DisplayOrder = 6,
                PluCode = "00006",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000600000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "KUZ-002",
                Name = "Kuzu Külbastı / But",
                CategoryId = catKuzu.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 580.00m,
                CostPrice = 420.00m,
                MinStockLevel = 10m,
                MaxStockLevel = 100m,
                ShelfLifeDays = 6,
                IsQuickButton = true,
                ButtonColor = "#DC143C",
                DisplayOrder = 7,
                PluCode = "00007",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000700000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "KUZ-003",
                Name = "Kuzu Gerdan (Haşlamalık)",
                CategoryId = catKuzu.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 440.00m,
                CostPrice = 310.00m,
                MinStockLevel = 10m,
                MaxStockLevel = 60m,
                ShelfLifeDays = 6,
                IsQuickButton = true,
                ButtonColor = "#CD5C5C",
                DisplayOrder = 8,
                PluCode = "00008",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000800000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },

            // ŞARKÜTERİ / İŞLENMİŞ
            new Product
            {
                Code = "SRK-001",
                Name = "Ev Yapımı Kasap Sucuğu",
                CategoryId = catSarkuteri.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 640.00m,
                CostPrice = 410.00m,
                MinStockLevel = 20m,
                MaxStockLevel = 200m,
                ShelfLifeDays = 60,
                IsQuickButton = true,
                ButtonColor = "#8B4513",
                DisplayOrder = 9,
                PluCode = "00009",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200000900000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "SRK-002",
                Name = "Kayseri Çemenli Pastırma",
                CategoryId = catSarkuteri.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 1200.00m,
                CostPrice = 850.00m,
                MinStockLevel = 5m,
                MaxStockLevel = 30m,
                ShelfLifeDays = 90,
                IsQuickButton = true,
                ButtonColor = "#A0522D",
                DisplayOrder = 10,
                PluCode = "00010",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200001000000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },

            // SAKATAT & YAN ÜRÜN
            new Product
            {
                Code = "SAK-001",
                Name = "Dana Ciğer (Yaprak / Taze)",
                CategoryId = catSakatat.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 460.00m,
                CostPrice = 300.00m,
                MinStockLevel = 5m,
                MaxStockLevel = 40m,
                ShelfLifeDays = 4,
                IsQuickButton = true,
                ButtonColor = "#556B2F",
                DisplayOrder = 11,
                PluCode = "00011",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200001100000", BarcodeType = BarcodeType.EmbeddedWeight } }
            },
            new Product
            {
                Code = "SAK-002",
                Name = "İlikli Dana Kemiği (Çorbalık)",
                CategoryId = catSakatat.Id,
                UnitOfMeasureId = unitKg.Id,
                ProductType = ProductType.Tartili,
                VatRate = 1.0m,
                SalePrice = 90.00m,
                CostPrice = 30.00m,
                MinStockLevel = 10m,
                MaxStockLevel = 80m,
                ShelfLifeDays = 7,
                IsQuickButton = true,
                ButtonColor = "#708090",
                DisplayOrder = 12,
                PluCode = "00012",
                Barcodes = new List<BarcodeDefinition> { new() { Barcode = "200001200000", BarcodeType = BarcodeType.EmbeddedWeight } }
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // 8. INITIAL INVENTORY (Depo Stok Bakiyeleri)
        var stockItems = new List<StockItem>();
        foreach (var p in products)
        {
            var isKiyma = p.Code == "DAN-004";
            var isKusbasi = p.Code == "DAN-003";

            // Near-expiry demonstration batches (Dana Kıyma 1 gün, Dana Kuşbaşı 1.5 gün kaldı)
            DateTime? shelfDate = isKiyma 
                ? DateTime.UtcNow.AddDays(1) 
                : (isKusbasi ? DateTime.UtcNow.AddHours(36) : DateTime.UtcNow.AddDays(p.ShelfLifeDays));

            var location = isKiyma ? "REYON-KIYMA-01" : (isKusbasi ? "REYON-KUŞBAŞI-02" : "REYON-TEZGAH-01");

            stockItems.Add(new StockItem
            {
                WarehouseId = mainWarehouse.Id,
                ProductId = p.Id,
                CurrentQuantity = isKiyma ? 28.5m : (isKusbasi ? 22.0m : 45.5m),
                ReservedQuantity = 0,
                LotNumber = isKiyma ? "LOT-2026-KYM-01" : (isKusbasi ? "LOT-2026-KSB-02" : "LOT-2026-0801"),
                ExpiryDate = DateTime.UtcNow.AddDays(p.ShelfLifeDays),
                ShelfDisplayExpiryDate = shelfDate,
                ShelfLocation = location,
                ReprocessingNotes = isKiyma 
                    ? "Reyon sergileme süresi bitmek üzere! Sucuk veya kasap köftesi için ideal hammadde." 
                    : (isKusbasi ? "Reyon süresi 1.5 gün kaldı. Kekikli & zeytinyağlı marinasyona uygun." : null)
            });
        }
        await context.StockItems.AddRangeAsync(stockItems);

        // 9. CUTTING TEMPLATES (Parçalama Ağacı BOM Şablonları)
        var danaTemplate = new CuttingTemplate
        {
            Name = "Dana Karkas Standart Parçalama Ağacı",
            AnimalType = AnimalType.Buyukbas,
            Description = "1 Tam Dana Karkasından (~300kg) çıkan standart randıman ve maliyet dağılımı",
            Items = new List<CuttingTemplateItem>
            {
                new() { CutName = "Dana Antrikot", TargetProductId = products.First(x => x.Code == "DAN-001").Id, ExpectedPercentage = 8.5m, CostWeightRatio = 1.6m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Dana Bonfile", TargetProductId = products.First(x => x.Code == "DAN-002").Id, ExpectedPercentage = 3.5m, CostWeightRatio = 2.4m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Dana Kuşbaşı / Kol / But", TargetProductId = products.First(x => x.Code == "DAN-003").Id, ExpectedPercentage = 28.0m, CostWeightRatio = 1.2m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Dana Kıyma / Döş", TargetProductId = products.First(x => x.Code == "DAN-004").Id, ExpectedPercentage = 30.0m, CostWeightRatio = 1.0m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Dana Ciğer / Sakatat", TargetProductId = products.First(x => x.Code == "SAK-001").Id, ExpectedPercentage = 5.0m, CostWeightRatio = 0.8m, IsByproduct = true, IsWaste = false },
                new() { CutName = "İlikli Kemik", TargetProductId = products.First(x => x.Code == "SAK-002").Id, ExpectedPercentage = 15.0m, CostWeightRatio = 0.2m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Kavram Yağı", ExpectedPercentage = 6.0m, CostWeightRatio = 0.3m, IsByproduct = true, IsWaste = false },
                new() { CutName = "Fire / Nem Kaybı", ExpectedPercentage = 4.0m, CostWeightRatio = 0.0m, IsByproduct = false, IsWaste = true }
            }
        };

        var kuzuTemplate = new CuttingTemplate
        {
            Name = "Kuzu Gövde Standart Parçalama Ağacı",
            AnimalType = AnimalType.Kucukbas,
            Description = "1 Tam Kuzu Gövdesinden (~25kg) parçalama reçetesi",
            Items = new List<CuttingTemplateItem>
            {
                new() { CutName = "Kuzu Pirzola", TargetProductId = products.First(x => x.Code == "KUZ-001").Id, ExpectedPercentage = 18.0m, CostWeightRatio = 1.8m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Kuzu But / Külbastı", TargetProductId = products.First(x => x.Code == "KUZ-002").Id, ExpectedPercentage = 34.0m, CostWeightRatio = 1.3m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Kuzu Gerdan", TargetProductId = products.First(x => x.Code == "KUZ-003").Id, ExpectedPercentage = 12.0m, CostWeightRatio = 0.9m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Kuzu Kol / Kuşbaşı", ExpectedPercentage = 20.0m, CostWeightRatio = 1.1m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Kuzu Kemik & Yağ", ExpectedPercentage = 12.0m, CostWeightRatio = 0.15m, IsByproduct = false, IsWaste = false },
                new() { CutName = "Fire (Doğal Kuruma)", ExpectedPercentage = 4.0m, CostWeightRatio = 0.0m, IsByproduct = false, IsWaste = true }
            }
        };

        await context.CuttingTemplates.AddRangeAsync(danaTemplate, kuzuTemplate);

        // 10. SAMPLE ANIMAL LOT (Giriş Yapılmış Örnek Karkas)
        var sampleLot = new AnimalLot
        {
            LotNumber = "LOT-2026-DANA-0823",
            AnimalType = AnimalType.Buyukbas,
            CarcassType = AnimalCarcassType.Karkas,
            SupplierName = "Afyon Besi Çiftliği A.Ş.",
            EarTagNumber = "TR0300998877",
            LiveWeightKg = 540.0m,
            CarcassWeightKg = 312.5m,
            PurchasePriceTotal = 93750.0m, // 300 TL / kg karkas
            Origin = "Afyonkarahisar / Çay",
            SlaughterDate = DateTime.UtcNow.AddDays(-1),
            VeterinaryReportNo = "VET-2026-44129",
            TraceabilityCode = "TR03-0823-3125",
            Notes = "1. Sınıf Dana Karkası, randıman yüksek bekleniyor.",
            IsProcessed = false
        };

        await context.AnimalLots.AddAsync(sampleLot);

        // 11. SAMPLE CUSTOMERS (Cari Hesaplar)
        var cust1 = new Customer
        {
            Code = "CAR-001",
            Name = "Saray Kebap & Izgara Restoranı",
            TaxNumberOrId = "7890123456",
            TaxOffice = "Kadıköy",
            Address = "Bağdat Caddesi No:142 Kadıköy / İstanbul",
            PhoneNumber = "05321112233",
            Email = "siparis@saraykebap.com",
            CurrentBalance = 14500.0m, // Borçlu
            CreditLimit = 50000.0m,
            PaymentTermDays = 15,
            IsCompany = true
        };

        var cust2 = new Customer
        {
            Code = "CAR-002",
            Name = "Hakan Demir",
            TaxNumberOrId = "12345678901",
            PhoneNumber = "05423334455",
            Address = "Atatürk Mah. Karanfil Sok. No:5",
            CurrentBalance = 0m,
            CreditLimit = 5000.0m,
            PaymentTermDays = 30,
            IsCompany = false
        };

        await context.Customers.AddRangeAsync(cust1, cust2);

        // 12. CASH REGISTER
        var cashRegister = new CashRegister
        {
            Name = "Ana Kasa (Dokunmatik POS)",
            Code = "KAS-01",
            WarehouseId = posStoreWarehouse.Id,
            CurrentBalance = 2500.0m
        };
        await context.CashRegisters.AddAsync(cashRegister);

        // 13. DEFAULT APP SETTINGS
        var defaultSettings = new List<AppSetting>
        {
            new() { Key = "Scale.Brand", Value = "CAS", Category = "Scale", Description = "Terazi Markası (CAS, DIGI, BAYKON, SIMULATOR)" },
            new() { Key = "Scale.Port", Value = "COM1", Category = "Scale", Description = "Seri Port Bağlantısı" },
            new() { Key = "Scale.BaudRate", Value = "9600", Category = "Scale", Description = "Baud Rate" },
            new() { Key = "Sms.Provider", Value = "MOCK", Category = "Sms", Description = "SMS Sağlayıcı (NETGSM, ILETI_MERKEZI, MOCK)" },
            new() { Key = "EInvoice.Integrator", Value = "MOCK_INTEGRATOR", Category = "EInvoice", Description = "E-Fatura Entegratörü (UYUMSOFT, LOGO, MOCK_INTEGRATOR)" },
            new() { Key = "Backup.AutoScheduleEnabled", Value = "True", Category = "Backup", Description = "Otomatik Zamanlanmış Yedekleme" },
            new() { Key = "Backup.Directory", Value = "C:\\KasapOtomasyon\\Yedekler", Category = "Backup", Description = "Yedek Klasörü" },
            new() { Key = "UI.Theme", Value = "Light", Category = "UI", Description = "Arayüz Teması (Dark / Light)" }
        };
        await context.AppSettings.AddRangeAsync(defaultSettings);

        // 14. SEED COLD STORAGE INFRASTRUCTURE FACILITIES
        var rooms = new List<ColdStorageRoom>
        {
            new() { RoomCode = "SH-01", RoomName = "Karkas Dinlendirme Soğuk Hava #1", StorageType = ColdStorageType.SogukHava, CurrentTemperature = 2.1m, TargetMinTemp = 0m, TargetMaxTemp = 4m, HumidityPercentage = 85m },
            new() { RoomCode = "SH-02", RoomName = "Sıcak Karkas Şoklama Odası #2", StorageType = ColdStorageType.SicakKarkasSoklama, CurrentTemperature = -1.5m, TargetMinTemp = -4m, TargetMaxTemp = 0m, HumidityPercentage = 90m },
            new() { RoomCode = "SH-03", RoomName = "Parçalama ve Paketleme Reyon Deposu", StorageType = ColdStorageType.ParcalamaReyonu, CurrentTemperature = 3.6m, TargetMinTemp = 0m, TargetMaxTemp = 6m, HumidityPercentage = 75m },
            new() { RoomCode = "SH-04", RoomName = "Sakatat ve Yan Ürün Deposu", StorageType = ColdStorageType.SakatatDeposu, CurrentTemperature = 1.8m, TargetMinTemp = 0m, TargetMaxTemp = 3m, HumidityPercentage = 88m }
        };
        await context.ColdStorageRooms.AddRangeAsync(rooms);

        await context.SaveChangesAsync();
    }

    public static async Task EnsureMezbahaTablesAndBreedsAsync(KasapDbContext context)
    {
        try
        {
            // 1. Ensure SQLite tables exist via raw DDL for seamless updates on existing databases
            var createTablesSql = @"
                CREATE TABLE IF NOT EXISTS AnimalBreeds (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    AnimalType INTEGER NOT NULL,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS AnimalIntakes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EarTagNumber TEXT NOT NULL,
                    PassportNumber TEXT,
                    AnimalType INTEGER NOT NULL,
                    Breed TEXT,
                    Gender INTEGER NOT NULL,
                    AgeMonths INTEGER NOT NULL,
                    LiveWeightKg REAL NOT NULL,
                    PurchasePrice REAL NOT NULL,
                    ProducerCustomerId INTEGER,
                    ProducerName TEXT,
                    WaybillNumber TEXT,
                    FarmOrigin TEXT,
                    ArrivalDate TEXT NOT NULL,
                    PlannedSlaughterDate TEXT,
                    SlaughterOrderNumber INTEGER NOT NULL,
                    BatchNumber TEXT,
                    RfidOrQrCode TEXT,
                    VeterinaryStatus INTEGER NOT NULL,
                    IsApprovedForSlaughter INTEGER NOT NULL,
                    SlaughterStatus INTEGER NOT NULL,
                    BlockReason TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS VeterinaryChecks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AnimalIntakeId INTEGER NOT NULL,
                    VeterinarianName TEXT NOT NULL,
                    DiplomaNumber TEXT,
                    CheckDate TEXT NOT NULL,
                    IsAntemortem INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    IsApprovedForSlaughter INTEGER NOT NULL,
                    BodyTemperature REAL NOT NULL,
                    Diagnosis TEXT,
                    QuarantineNotes TEXT,
                    ReportNumber TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS SlaughterRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SlaughterNumber TEXT NOT NULL,
                    CarcassNumber TEXT NOT NULL,
                    AnimalIntakeId INTEGER NOT NULL,
                    SlaughterDate TEXT NOT NULL,
                    ButcherPersonName TEXT,
                    VeterinarianName TEXT,
                    SlaughterLine TEXT,
                    LiveWeightKg REAL NOT NULL,
                    HotCarcassWeightKg REAL NOT NULL,
                    ColdCarcassWeightKg REAL NOT NULL,
                    HeadWeightKg REAL NOT NULL,
                    HideWeightKg REAL NOT NULL,
                    OffalWeightKg REAL NOT NULL,
                    FatWeightKg REAL NOT NULL,
                    BoneWeightKg REAL NOT NULL,
                    SlaughterWasteKg REAL NOT NULL,
                    AnimalPurchaseCost REAL NOT NULL,
                    SlaughterLaborCost REAL NOT NULL,
                    TransportationCost REAL NOT NULL,
                    CoolingElectricityCost REAL NOT NULL,
                    GeneralOverheadCost REAL NOT NULL,
                    ColdStorageRoomId INTEGER,
                    ColdStorageLocation TEXT,
                    HookRailNumber TEXT DEFAULT 'Ray-1 / Askı-14',
                    ConformationClass TEXT DEFAULT 'R',
                    FatCoverScore INTEGER DEFAULT 3,
                    MarblingScore INTEGER DEFAULT 5,
                    PostMortemPh24 REAL DEFAULT 5.65,
                    Status INTEGER NOT NULL,
                    IsDeboned INTEGER NOT NULL,
                    BarcodeOrQr TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ColdStorageRooms (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RoomCode TEXT NOT NULL,
                    RoomName TEXT NOT NULL,
                    StorageType INTEGER NOT NULL,
                    CurrentTemperature REAL NOT NULL,
                    TargetMinTemp REAL NOT NULL,
                    TargetMaxTemp REAL NOT NULL,
                    HumidityPercentage REAL NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS WasteLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WasteNumber TEXT NOT NULL,
                    CauseType INTEGER NOT NULL,
                    ProductId INTEGER,
                    ProductName TEXT,
                    LotOrCarcassNumber TEXT,
                    WeightKg REAL NOT NULL,
                    UnitCost REAL NOT NULL,
                    ResponsiblePerson TEXT,
                    ApprovedByPerson TEXT,
                    Description TEXT,
                    LogDate TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ForensicAnomalyRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AnomalyCode TEXT NOT NULL,
                    Type INTEGER NOT NULL,
                    Severity TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    ReferenceNumber TEXT,
                    ExpectedQuantity REAL NOT NULL,
                    ActualQuantity REAL NOT NULL,
                    FinancialLossEstimated REAL NOT NULL,
                    DetectedDate TEXT NOT NULL,
                    IsResolved INTEGER NOT NULL,
                    ResolutionNotes TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS SlaughterServiceInvoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT NOT NULL,
                    ProducerCustomerId INTEGER NOT NULL,
                    AnimalIntakeId INTEGER NOT NULL,
                    SlaughterFeePerHead REAL NOT NULL,
                    SlaughterFeePerKg REAL NOT NULL,
                    DeboningServiceFee REAL NOT NULL,
                    PackagingServiceFee REAL NOT NULL,
                    ColdStorageServiceFee REAL NOT NULL,
                    PaidAmount REAL NOT NULL,
                    IsPaid INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS CarcassDeboningCuts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SlaughterRecordId INTEGER NOT NULL,
                    CarcassNumber TEXT NOT NULL,
                    EarTagNumber TEXT NOT NULL,
                    AnatomicalRegion TEXT NOT NULL,
                    CutName TEXT NOT NULL,
                    WeightKg REAL NOT NULL,
                    YieldPercentage REAL NOT NULL,
                    TheoreticalStandardRatio REAL DEFAULT 0,
                    VariancePercentage REAL DEFAULT 0,
                    QualityGrade TEXT NOT NULL,
                    UnitCostEstimated REAL NOT NULL,
                    TotalCutValue REAL NOT NULL,
                    LotNumber TEXT NOT NULL,
                    Barcode TEXT NOT NULL,
                    Gs1Barcode128 TEXT DEFAULT '',
                    TargetStorageLocation TEXT NOT NULL,
                    CutDate TEXT NOT NULL,
                    MasterButcher TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Tenants (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    HoldingName TEXT,
                    CountryCode TEXT NOT NULL,
                    TaxNumber TEXT,
                    ContactEmail TEXT,
                    ContactPhone TEXT,
                    DefaultLanguage TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Companies (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL,
                    Code TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    CountryCode TEXT NOT NULL,
                    CurrencyCode TEXT NOT NULL,
                    DefaultLanguage TEXT NOT NULL,
                    TaxOffice TEXT,
                    TaxNumber TEXT,
                    Address TEXT,
                    LogoUrl TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Plants (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL,
                    CompanyId INTEGER NOT NULL,
                    Code TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    PlantType INTEGER NOT NULL,
                    OfficialApprovalNumber TEXT,
                    Address TEXT,
                    City TEXT,
                    CountryCode TEXT NOT NULL,
                    Latitude REAL DEFAULT 0,
                    Longitude REAL DEFAULT 0,
                    PlantManagerName TEXT,
                    Phone TEXT,
                    IpSubnet TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS FacilityUnits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL,
                    CompanyId INTEGER NOT NULL,
                    PlantId INTEGER NOT NULL,
                    Code TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    UnitType INTEGER NOT NULL,
                    TargetTemperatureCelsius REAL DEFAULT 0,
                    AssignedSupervisor TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS HaccpControlPoints (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    CcpCode TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    ProcessStep TEXT NOT NULL,
                    HazardType INTEGER NOT NULL,
                    HazardDescription TEXT NOT NULL,
                    CriticalMinLimit REAL NOT NULL,
                    CriticalMaxLimit REAL NOT NULL,
                    UnitOfLimit TEXT NOT NULL,
                    MonitoringFrequencyMinutes INTEGER NOT NULL,
                    CorrectiveActionProcedure TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS HaccpInspectionRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    HaccpControlPointId INTEGER NOT NULL,
                    TargetIdentifier TEXT NOT NULL,
                    MeasuredValue REAL NOT NULL,
                    IsViolation INTEGER NOT NULL DEFAULT 0,
                    AutoHoldTriggered INTEGER NOT NULL DEFAULT 0,
                    InspectorName TEXT NOT NULL,
                    CorrectiveActionNotes TEXT,
                    InspectionTimestamp TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS FoodRecallCases (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    CaseNumber TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    TriggerReason TEXT NOT NULL,
                    RiskLevel INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    RootCauseEarTag TEXT NOT NULL,
                    RootCauseCarcassNumber TEXT NOT NULL,
                    RootCauseLotNumber TEXT NOT NULL,
                    AffectedLotCount INTEGER NOT NULL DEFAULT 0,
                    AffectedCustomerCount INTEGER NOT NULL DEFAULT 0,
                    TotalRecalledWeightKg REAL NOT NULL DEFAULT 0,
                    EstimatedFinancialLoss REAL NOT NULL DEFAULT 0,
                    InitiatedByUserName TEXT NOT NULL,
                    InitiatedDate TEXT NOT NULL,
                    ClosedDate TEXT,
                    ClosureResolution TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS FoodRecallItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    FoodRecallCaseId INTEGER NOT NULL,
                    LotNumber TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    QuantityKg REAL NOT NULL,
                    WarehouseLocation TEXT NOT NULL,
                    CustomerName TEXT,
                    InvoiceNumber TEXT,
                    ItemState INTEGER NOT NULL,
                    ResolutionNotes TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ProcessingRecipes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    RecipeCode TEXT NOT NULL,
                    RecipeName TEXT NOT NULL,
                    Category INTEGER NOT NULL,
                    StandardBatchWeightKg REAL NOT NULL,
                    ExpectedYieldPercentage REAL NOT NULL,
                    Description TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ProcessingRecipeItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProcessingRecipeId INTEGER NOT NULL,
                    ItemType INTEGER NOT NULL,
                    ItemName TEXT NOT NULL,
                    PercentageRatio REAL NOT NULL,
                    QuantityPerBatchKg REAL NOT NULL,
                    StandardUnitCost REAL NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS PalletSSCCs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    Sscc18Barcode TEXT NOT NULL,
                    PalletNumber TEXT NOT NULL,
                    LotNumber TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    TotalBoxesCount INTEGER NOT NULL,
                    TotalNetWeightKg REAL NOT NULL,
                    TotalGrossWeightKg REAL NOT NULL,
                    TargetWarehouseBin TEXT NOT NULL,
                    DestinationCustomer TEXT,
                    IsAllocatedForShipment INTEGER NOT NULL DEFAULT 0,
                    IsQuarantined INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS TrueCostRollups (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    CarcassNumber TEXT NOT NULL,
                    EarTagNumber TEXT NOT NULL,
                    AnimalPurchaseCost REAL NOT NULL,
                    LivestockTransportCost REAL NOT NULL,
                    SlaughterLaborCost REAL NOT NULL,
                    VeterinaryInspectionCost REAL NOT NULL,
                    ColdStorageChillingEnergyCost REAL NOT NULL,
                    DeboningLaborCost REAL NOT NULL,
                    PackagingAndLabelingCost REAL NOT NULL,
                    GeneralFacilityOverheadCost REAL NOT NULL,
                    HideLeatherCreditRevenue REAL NOT NULL,
                    OffalOrganCreditRevenue REAL NOT NULL,
                    BoneAndFatCreditRevenue REAL NOT NULL,
                    ColdCarcassWeightKg REAL NOT NULL,
                    TotalDebonedMeatSalesValue REAL NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS RetailScaleBarcodeRules (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    RuleName TEXT NOT NULL,
                    Prefix TEXT NOT NULL,
                    PluLength INTEGER NOT NULL,
                    ValueLength INTEGER NOT NULL,
                    IsWeightType INTEGER NOT NULL DEFAULT 1,
                    DecimalPlaces INTEGER NOT NULL,
                    HasCheckDigit INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS FraudInvestigationCases (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    CaseCode TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    RiskLevel INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    TriggerAction TEXT NOT NULL,
                    SuspectedUserName TEXT NOT NULL,
                    DiscrepancyQuantityKg REAL NOT NULL,
                    EstimatedFinancialImpact REAL NOT NULL,
                    EvidencePayload TEXT NOT NULL,
                    AssignedInvestigator TEXT,
                    ResolutionNotes TEXT,
                    ResolvedAt TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS CarcassPortions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    SlaughterRecordId INTEGER NOT NULL,
                    PortionCode TEXT NOT NULL,
                    PortionType INTEGER NOT NULL,
                    WeightKg REAL NOT NULL,
                    HookRailLocation TEXT NOT NULL,
                    IsDeboned INTEGER NOT NULL DEFAULT 0,
                    Gs1Barcode TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ChillingRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    SlaughterRecordId INTEGER NOT NULL,
                    ColdRoomCode TEXT NOT NULL,
                    RailAndHookNumber TEXT NOT NULL,
                    EntryTime TEXT NOT NULL,
                    ExitTime TEXT,
                    EntryHotWeightKg REAL NOT NULL,
                    ExitColdWeightKg REAL NOT NULL,
                    AverageRoomTemperatureCelsius REAL NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS CuttingOrders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    OrderCode TEXT NOT NULL,
                    SlaughterRecordId INTEGER NOT NULL,
                    CarcassNumber TEXT NOT NULL,
                    EarTagNumber TEXT NOT NULL,
                    MasterButcher TEXT NOT NULL,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT,
                    InputCarcassWeightKg REAL NOT NULL,
                    TotalPrimalCutsWeightKg REAL NOT NULL,
                    TotalByproductsWeightKg REAL NOT NULL,
                    TotalBoneAndFatWeightKg REAL NOT NULL,
                    TotalWasteWeightKg REAL NOT NULL,
                    ProcessLossWeightKg REAL NOT NULL,
                    Status INTEGER NOT NULL,
                    VarianceExplanation TEXT,
                    SupervisorApprovedBy TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS IoTTelemetryLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    DeviceCode TEXT NOT NULL,
                    LocationName TEXT NOT NULL,
                    TemperatureCelsius REAL NOT NULL,
                    HumidityPercentage REAL NOT NULL,
                    IsDoorOpen INTEGER NOT NULL DEFAULT 0,
                    IsPowerOn INTEGER NOT NULL DEFAULT 1,
                    IsThresholdViolation INTEGER NOT NULL DEFAULT 0,
                    ViolationMessage TEXT,
                    TelemetryTimestamp TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS MeatProcessingEquipments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    EquipmentCode TEXT NOT NULL,
                    EquipmentName TEXT NOT NULL,
                    EquipmentType TEXT NOT NULL,
                    CapacityInfo TEXT NOT NULL,
                    OperatingStatus TEXT NOT NULL,
                    LastSanitizationDate TEXT NOT NULL,
                    SanitizedBy TEXT NOT NULL,
                    SanitizingAgent TEXT NOT NULL,
                    IsSanitizedAndReady INTEGER NOT NULL DEFAULT 1,
                    NextMaintenanceDate TEXT,
                    Notes TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS AuxiliaryMaterialStocks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    MaterialCode TEXT NOT NULL,
                    MaterialName TEXT NOT NULL,
                    ItemType INTEGER NOT NULL,
                    CurrentStock REAL NOT NULL,
                    UnitOfMeasure TEXT NOT NULL,
                    UnitCost REAL NOT NULL,
                    MinStockLevel REAL NOT NULL,
                    ExpiryDate TEXT,
                    StorageCondition TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ReprocessingQualityChecks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    InspectionNumber TEXT NOT NULL,
                    BatchLotNumber TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    MeasuredPh REAL NOT NULL,
                    MeasuredWaterActivityAw REAL NOT NULL,
                    MeasuredCoreTempCelsius REAL NOT NULL,
                    QualityVerdict TEXT NOT NULL,
                    IsApproved INTEGER NOT NULL DEFAULT 1,
                    IsAutoBlocked INTEGER NOT NULL DEFAULT 0,
                    InspectorName TEXT NOT NULL,
                    Notes TEXT,
                    InspectionDate TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS SucukCuringBatches (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    CuringLotNumber TEXT NOT NULL,
                    RecipeName TEXT NOT NULL,
                    ChamberRoomCode TEXT NOT NULL,
                    InitialGreenWeightKg REAL NOT NULL,
                    CurrentWeightKg REAL NOT NULL,
                    TargetDryWeightKg REAL NOT NULL,
                    InitialPh REAL NOT NULL,
                    CurrentPh REAL NOT NULL,
                    ChamberTemperatureCelsius REAL NOT NULL,
                    ChamberHumidityRh REAL NOT NULL,
                    ElapsedDays INTEGER NOT NULL DEFAULT 0,
                    TargetCuringDays INTEGER NOT NULL DEFAULT 10,
                    Status TEXT NOT NULL,
                    ResponsibleButcher TEXT NOT NULL,
                    Notes TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ReprocessingDisposalRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    ProtocolNumber TEXT NOT NULL,
                    BatchLotNumber TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    DisposedQuantityKg REAL NOT NULL,
                    EstimatedFinancialLossLira REAL NOT NULL,
                    DisposalReason TEXT NOT NULL,
                    MeasuredPh REAL NOT NULL,
                    SensoryFailureNotes TEXT NOT NULL,
                    FirstApprover TEXT NOT NULL,
                    SecondApprover TEXT NOT NULL,
                    ApprovalDate TEXT NOT NULL,
                    RenderingCompanyName TEXT NOT NULL,
                    WaybillNumber TEXT NOT NULL,
                    DisposalStatus TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ButcherWasteIncentives (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenantId INTEGER NOT NULL DEFAULT 1,
                    CompanyId INTEGER NOT NULL DEFAULT 1,
                    PlantId INTEGER NOT NULL DEFAULT 1,
                    ButcherName TEXT NOT NULL,
                    RoleTitle TEXT NOT NULL,
                    PeriodMonth TEXT NOT NULL,
                    TotalRescuedMeatKg REAL NOT NULL,
                    TotalNetValueCreatedLira REAL NOT NULL,
                    IncentiveRatePercentage REAL NOT NULL DEFAULT 2.0,
                    TotalBatchesReprocessed INTEGER NOT NULL DEFAULT 0,
                    BadgeTitle TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );
            ";

            await context.Database.ExecuteSqlRawAsync(createTablesSql);

            // Execute ALTER TABLE safely to upgrade any existing SQLite database schema
            var alterStatements = new[]
            {
                "ALTER TABLE Roles ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE Users ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE Users ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE Users ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE Users ADD COLUMN PreferredLanguage TEXT DEFAULT 'tr-TR';",
                "ALTER TABLE AuditLogs ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE AuditLogs ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE AuditLogs ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE AuditLogs ADD COLUMN Reason TEXT DEFAULT '';",
                "ALTER TABLE AuditLogs ADD COLUMN SupervisorApprover TEXT DEFAULT '';",
                "ALTER TABLE AuditLogs ADD COLUMN WorkstationMachineId TEXT DEFAULT '';",
                "ALTER TABLE AuditLogs ADD COLUMN ApplicationVersion TEXT DEFAULT '2.0.0';",
                "ALTER TABLE AuditLogs ADD COLUMN Sha256Signature TEXT DEFAULT '';",
                "ALTER TABLE AnimalIntakes ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE AnimalIntakes ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE AnimalIntakes ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE AnimalIntakes ADD COLUMN LivestockTransportTripId INTEGER NULL;",
                "ALTER TABLE VeterinaryChecks ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE VeterinaryChecks ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE VeterinaryChecks ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE SlaughterRecords ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE SlaughterRecords ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE SlaughterRecords ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE SlaughterRecords ADD COLUMN ConformationClass TEXT DEFAULT 'R';",
                "ALTER TABLE SlaughterRecords ADD COLUMN FatCoverScore INTEGER DEFAULT 3;",
                "ALTER TABLE SlaughterRecords ADD COLUMN MarblingScore INTEGER DEFAULT 5;",
                "ALTER TABLE SlaughterRecords ADD COLUMN PostMortemPh24 REAL DEFAULT 5.65;",
                "ALTER TABLE SlaughterRecords ADD COLUMN HookRailNumber TEXT DEFAULT 'Ray-1 / Askı-14';",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN CuttingOrderId INTEGER NULL;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN TheoreticalStandardRatio REAL DEFAULT 0;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN VariancePercentage REAL DEFAULT 0;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN Gs1Barcode128 TEXT DEFAULT '';",
                "ALTER TABLE WasteLogs ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE WasteLogs ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE WasteLogs ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE ColdStorageRooms ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE ColdStorageRooms ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE ColdStorageRooms ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE ForensicAnomalyRecords ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE ForensicAnomalyRecords ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE ForensicAnomalyRecords ADD COLUMN PlantId INTEGER DEFAULT 1;",
                "ALTER TABLE SlaughterServiceInvoices ADD COLUMN TenantId INTEGER DEFAULT 1;",
                "ALTER TABLE SlaughterServiceInvoices ADD COLUMN CompanyId INTEGER DEFAULT 1;",
                "ALTER TABLE SlaughterServiceInvoices ADD COLUMN PlantId INTEGER DEFAULT 1;"
            };

            foreach (var alter in alterStatements)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(alter);
                }
                catch { } // Column already exists in newly created databases
            }

            // 2. Comprehensive World Animal Breeds Catalog (65+ Breeds)
            var count = await context.AnimalBreeds.CountAsync();
            if (count == 0)
            {
                var worldBreeds = new List<AnimalBreed>
                {
                    // --- BÜYÜKBAŞ: DÜNYA VE TÜRKİYE SIĞIR IRKLARI ---
                    new() { Name = "Simental (Simmental)", AnimalType = AnimalType.Buyukbas, Description = "İsviçre / Almanya - Yüksek süt ve et verimi" },
                    new() { Name = "Aberdeen Angus (Black Angus)", AnimalType = AnimalType.Buyukbas, Description = "İskoçya - Dünyanın en popüler premium mermersi et ırkı" },
                    new() { Name = "Red Angus", AnimalType = AnimalType.Buyukbas, Description = "İskoçya / ABD - Kaliteli etçi sığır ırkı" },
                    new() { Name = "Holstein (Siyah-Alaca)", AnimalType = AnimalType.Buyukbas, Description = "Hollanda / Almanya - Yüksek süt ve karkas verimi" },
                    new() { Name = "Hereford (Boynuzsuz / Polled)", AnimalType = AnimalType.Buyukbas, Description = "İngiltere - Mera besisine son derece uygun et ırkı" },
                    new() { Name = "Şarole (Charolais)", AnimalType = AnimalType.Buyukbas, Description = "Fransa - Yüksek karkas ağırlığı ve düşük yağ oranı" },
                    new() { Name = "Limuzin (Limousin)", AnimalType = AnimalType.Buyukbas, Description = "Fransa - Yüksek randımanlı kasaplık et ırkı" },
                    new() { Name = "Montofon / Brown Swiss (İsviçre Esmeri)", AnimalType = AnimalType.Buyukbas, Description = "İsviçre - Dayanıklı kombine ırk" },
                    new() { Name = "Wagyu / Kobe (Kuroge Washu)", AnimalType = AnimalType.Buyukbas, Description = "Japonya - A5 mermersi yağ dokulu dünyanın en değerli eti" },
                    new() { Name = "Belçika Mavisi (Belgian Blue)", AnimalType = AnimalType.Buyukbas, Description = "Belçika - Çift kas yapılı ultra yüksek karkas randımanı" },
                    new() { Name = "Piedmontese (Piyemonte)", AnimalType = AnimalType.Buyukbas, Description = "İtalya - Yağsız, yumuşak çift kaslı et ırkı" },
                    new() { Name = "Chianina", AnimalType = AnimalType.Buyukbas, Description = "İtalya (Toskana) - Dünyanın en büyük ve uzun boylu sığır ırkı" },
                    new() { Name = "Brahman (Zebu)", AnimalType = AnimalType.Buyukbas, Description = "Hindistan / ABD - Sıcağa ve parazitlere dayanıklı hörgüçlü ırk" },
                    new() { Name = "Brangus (Brahman x Angus)", AnimalType = AnimalType.Buyukbas, Description = "ABD - Dayanıklı etçi melez ırk" },
                    new() { Name = "Santa Gertrudis", AnimalType = AnimalType.Buyukbas, Description = "ABD (Teksas) - Sıcak iklime dayanıklı et ırkı" },
                    new() { Name = "Blonde d'Aquitaine", AnimalType = AnimalType.Buyukbas, Description = "Fransa - Ağır karkaslı açık renkli et ırkı" },
                    new() { Name = "Aubrac", AnimalType = AnimalType.Buyukbas, Description = "Fransa - Dağlık bölge dayanıklı et ırkı" },
                    new() { Name = "Salers", AnimalType = AnimalType.Buyukbas, Description = "Fransa - Kırmızı renkli rustik et ve süt ırkı" },
                    new() { Name = "Gelbvieh", AnimalType = AnimalType.Buyukbas, Description = "Almanya - Hızlı büyüyen etçi ırk" },
                    new() { Name = "Highland Cattle (İskoç Yayla Sığırı)", AnimalType = AnimalType.Buyukbas, Description = "İskoçya - Uzun tüylü, soğuğa dayanıklı doğal mera eti" },
                    new() { Name = "Galloway / Belted Galloway", AnimalType = AnimalType.Buyukbas, Description = "İskoçya - Kuşaklı dayanıklı et sığırı" },
                    new() { Name = "Texas Longhorn", AnimalType = AnimalType.Buyukbas, Description = "ABD - Uzun boynuzlu dayanıklı geleneksel ırk" },
                    new() { Name = "Jersey", AnimalType = AnimalType.Buyukbas, Description = "İngiltere (Jersey Adası) - Yüksek yağlı süt ırkı" },
                    new() { Name = "Guernsey", AnimalType = AnimalType.Buyukbas, Description = "İngiltere - Zengin süt ırkı" },
                    new() { Name = "Ayrshire", AnimalType = AnimalType.Buyukbas, Description = "İskoçya - Kırmızı-beyaz kombine ırk" },
                    new() { Name = "Doğu Anadolu Kırmızısı (DAK)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye - Doğu Anadolu yerli dayanıklı ırkı" },
                    new() { Name = "Güney Anadolu Kırmızısı (GAK)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye - Güneydoğu yerli ırkı" },
                    new() { Name = "Yerli Kara (Anadolu Karası)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye - İç Anadolu yerli ırkı" },
                    new() { Name = "Boz Irk (Plevne / Step)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye (Marmara / Ege) - Doğal otlayan lezzetli et ırkı" },
                    new() { Name = "Zavot Sığırı", AnimalType = AnimalType.Buyukbas, Description = "Türkiye (Kars / Ardahan) - Yüksek rakım sığırı" },
                    new() { Name = "Anadolu Mandası (Water Buffalo)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye - Kaymak ve lezzetli manda eti" },
                    new() { Name = "Murrah Mandası", AnimalType = AnimalType.Buyukbas, Description = "Hindistan - Yüksek verimli dünya mandası" },

                    // --- KÜÇÜKBAŞ: DÜNYA VE TÜRKİYE KOYUN IRKLARI ---
                    new() { Name = "Merinos (Avustralya & Karacabey)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye / İspanya - İnce yün ve kaliteli et koyunu" },
                    new() { Name = "Kıvırcık (Trakya & Marmara)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Yağı ete homojen dağılan en lezzetli yerli et ırkı" },
                    new() { Name = "Akkaraman (Kangal & Karakaş)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Yağlı kuyruklu İç Anadolu ırkı" },
                    new() { Name = "Morkaraman (Doğu Anadolu)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Soğuğa dayanıklı Doğu Anadolu koyunu" },
                    new() { Name = "İvesi (Awassi)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye / Ortadoğu - Yüksek süt ve et verimli koyun" },
                    new() { Name = "Sakız (Chios)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye / Ege - Çoklu doğum ve yüksek süt verimi" },
                    new() { Name = "Tahirova", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Doğu Friz x Kıvırcık melezi et ve süt koyunu" },
                    new() { Name = "Dorper (Beyaz & Siyah Başlı)", AnimalType = AnimalType.Kucukbas, Description = "Güney Afrika - Yapağı döken, hızlı büyüyen dünya lideri et koyunu" },
                    new() { Name = "Suffolk", AnimalType = AnimalType.Kucukbas, Description = "İngiltere - Siyah başlı hızlı büyüyen et ırkı" },
                    new() { Name = "Texel", AnimalType = AnimalType.Kucukbas, Description = "Hollanda - Yüksek kaslı, yağsız karkas koyun ırkı" },
                    new() { Name = "Hampshire Down", AnimalType = AnimalType.Kucukbas, Description = "İngiltere - Ağır karkas et koyunu" },
                    new() { Name = "Romanov", AnimalType = AnimalType.Kucukbas, Description = "Rusya - Yılda 2 kez 3-4 kuzu veren kuzu fabrikası ırk" },
                    new() { Name = "Doğu Friz (East Friesian)", AnimalType = AnimalType.Kucukbas, Description = "Almanya - Dünyanın en yüksek süt verimli koyunu" },
                    new() { Name = "Assaf", AnimalType = AnimalType.Kucukbas, Description = "İsrail / İspanya - İvesi x Doğu Friz melezi süt ve et ırkı" },
                    new() { Name = "Lacaune", AnimalType = AnimalType.Kucukbas, Description = "Fransa - Rokfor peyniri ve et koyunu" },
                    new() { Name = "Charollais Koyunu", AnimalType = AnimalType.Kucukbas, Description = "Fransa - Yüksek karkas randımanı" },
                    new() { Name = "Ile de France", AnimalType = AnimalType.Kucukbas, Description = "Fransa - Üst düzey etçi merinos melezi" },
                    new() { Name = "Karagül (Astragan)", AnimalType = AnimalType.Kucukbas, Description = "Orta Asya - Kürk ve et koyunu" },
                    new() { Name = "Dağlıç", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - İç Batı Anadolu yuvarlak yağlı kuyruklu ırk" },
                    new() { Name = "Hemşin Koyunu", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Karadeniz yerli koyunu" },
                    new() { Name = "Norduz Koyunu", AnimalType = AnimalType.Kucukbas, Description = "Türkiye (Van / Gürpınar) - Dayanıklı et koyunu" },
                    new() { Name = "Çine Çaparı", AnimalType = AnimalType.Kucukbas, Description = "Türkiye (Aydın) - Yağlı kuyruklu yerli ırk" },

                    // --- KÜÇÜKBAŞ: DÜNYA VE TÜRKİYE KEÇİ IRKLARI ---
                    new() { Name = "Saanen (İsviçre Beyazı)", AnimalType = AnimalType.Kucukbas, Description = "İsviçre - Dünyanın en ünlü yüksek süt keçisi" },
                    new() { Name = "Boer Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Güney Afrika - Dünyanın 1 numaralı ağır karkas etçi keçisi" },
                    new() { Name = "Kıl Keçisi (Kara Keçi / Toros)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Çalı ve makilik alanları değerlendiren yerli keçi" },
                    new() { Name = "Halep (Şam / Damascus Keçisi)", AnimalType = AnimalType.Kucukbas, Description = "Suriye / Türkiye - Yüksek süt ve et verimli uzun kulaklı ırk" },
                    new() { Name = "Honamlı Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Türkiye (Toroslar) - Türkiye'nin en yüksek karkas ağırlıklı etçi keçisi" },
                    new() { Name = "Alpin (Fransız Alpin)", AnimalType = AnimalType.Kucukbas, Description = "Fransa - Yüksek dağ ve süt keçisi" },
                    new() { Name = "Toggenburg", AnimalType = AnimalType.Kucukbas, Description = "İsviçre - Soğuğa dayanıklı kahverengi-beyaz süt keçisi" },
                    new() { Name = "Anglo-Nubian (Nubya)", AnimalType = AnimalType.Kucukbas, Description = "İngiltere / Mısır - Yüksek yağlı süt ve et keçisi" },
                    new() { Name = "Tiftik Keçisi (Ankara Keçisi / Mohair)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Dünyaca ünlü parlak tiftik ve et keçisi" },
                    new() { Name = "Kilis Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Türkiye - Güneydoğu süt keçisi" },
                    new() { Name = "Maltız Keçisi (Malta)", AnimalType = AnimalType.Kucukbas, Description = "Malta - Çiftlik ortamına uygun süt keçisi" },
                    new() { Name = "Kalahari Red", AnimalType = AnimalType.Kucukbas, Description = "Güney Afrika - Kırmızı renkli dayanıklı et keçisi" },
                    new() { Name = "Kaşmir Keçisi (Cashmere)", AnimalType = AnimalType.Kucukbas, Description = "Himalayalar - Değerli kaşmir lifi ve et keçisi" }
                };

                await context.AnimalBreeds.AddRangeAsync(worldBreeds);
                await context.SaveChangesAsync();
            }

            // 3. SEED HACCP CRITICAL CONTROL POINTS (CCP-1 .. CCP-6)
            var ccpCount = await context.HaccpControlPoints.CountAsync();
            if (ccpCount == 0)
            {
                var ccps = new List<HaccpControlPoint>
                {
                    new()
                    {
                        CcpCode = "CCP-1",
                        Name = "Canlı Hayvan Ante-Mortem Muayenesi & Vücut Sıcaklığı",
                        ProcessStep = "Hayvan Kabul & Padok",
                        HazardType = HaccpHazardType.Biological,
                        HazardDescription = "Zoonoz veya bulaşıcı hastalık taşıyan hayvanların kesim hattına girmesi",
                        CriticalMinLimit = 38.0m,
                        CriticalMaxLimit = 39.5m,
                        UnitOfLimit = "°C",
                        MonitoringFrequencyMinutes = 15,
                        CorrectiveActionProcedure = "Hayvanı derhal Karantina Padoğuna al, resmi veteriner hekim raporu düzenle ve kesimi bloke et."
                    },
                    new()
                    {
                        CcpCode = "CCP-2",
                        Name = "Karkas 24 Saat İç Sıcaklığı & pH Kontrolü",
                        ProcessStep = "Monoray Soğutma Deposu",
                        HazardType = HaccpHazardType.Temperature,
                        HazardDescription = "Yetersiz soğutmada patojen bakteri üremesi ve et bozulması",
                        CriticalMinLimit = 0.0m,
                        CriticalMaxLimit = 4.0m,
                        UnitOfLimit = "°C",
                        MonitoringFrequencyMinutes = 60,
                        CorrectiveActionProcedure = "Karkası acil derin dondurucu / şok odasına al, mikrobiyolojik numune al ve depoyu karantinaya mühürle."
                    },
                    new()
                    {
                        CcpCode = "CCP-3",
                        Name = "Parçalama Salonu Ortam Sıcaklığı",
                        ProcessStep = "Karkas Parçalama & Kemiksizleştirme",
                        HazardType = HaccpHazardType.Temperature,
                        HazardDescription = "Parçalama sırasında ortam ısınması nedeniyle mikrobiyal çoğalma",
                        CriticalMinLimit = 0.0m,
                        CriticalMaxLimit = 12.0m,
                        UnitOfLimit = "°C",
                        MonitoringFrequencyMinutes = 30,
                        CorrectiveActionProcedure = "Parçalama hattını durdur, HVAC soğutma grubunu devreye al, etleri soğuk depoya geri çek."
                    },
                    new()
                    {
                        CcpCode = "CCP-4",
                        Name = "Metal Dedektör & X-Ray Yabancı Madde Taraması",
                        ProcessStep = "Paketleme & GS1 Etiketleme",
                        HazardType = HaccpHazardType.Physical,
                        HazardDescription = "Bıçak ucu, tel, metal veya kemik parçası kalıntısı",
                        CriticalMinLimit = 0.0m,
                        CriticalMaxLimit = 0.0m,
                        UnitOfLimit = "Tespit Sayısı",
                        MonitoringFrequencyMinutes = 10,
                        CorrectiveActionProcedure = "Paketi derhal reddet (eject), hattı durdur, kalibrasyon test bloğu ile dedektörü yeniden doğrula."
                    }
                };

                await context.HaccpControlPoints.AddRangeAsync(ccps);
                await context.SaveChangesAsync();
            }

            // 4. SEED MASTER PROCESSING RECIPES (BOM REÇETELERİ - SUCUK, KÖFTE, SOSLU ET)
            var recipeCount = await context.ProcessingRecipes.CountAsync();
            if (recipeCount == 0)
            {
                // 1. Geleneksel Fermente Sucuk (+60 Gün SKT)
                var sucukRecipe = new ProcessingRecipe
                {
                    RecipeCode = "RCP-SUCUK-01",
                    RecipeName = "Geleneksel Fermente Kasap Kangal Sucuğu",
                    Category = RecipeProductCategory.DryFermentedSausage,
                    StandardBatchWeightKg = 100.0m,
                    ExpectedYieldPercentage = 88.0m, // %12 kuruma & olgunlaşma firesi
                    ShelfLifeExtensionDays = 60,
                    TargetProductName = "Ev Yapımı Kasap Sucuğu",
                    Description = "Doğal sığır bağırsağında, sarımsak ve özel baharatlı fermente sucuk",
                    ProcessInstructions = "Et ve yağ 3 mm ayna ile çekilir. Baharat harcı ve kaya tuzu ile yoğrulur. 24 saat +4°C dinlendirilir. Doğal bağırsağa doldurulur."
                };
                await context.ProcessingRecipes.AddAsync(sucukRecipe);
                await context.SaveChangesAsync();

                var sucukItems = new List<ProcessingRecipeItem>
                {
                    new() { ProcessingRecipeId = sucukRecipe.Id, ItemType = RecipeItemType.MeatRawMaterial, ItemName = "Dana Döş / Tranç Eti", PercentageRatio = 70.0m, QuantityPerBatchKg = 70.0m, StandardUnitCost = 420.0m, Description = "Ana protein ve et dokusu (%70)" },
                    new() { ProcessingRecipeId = sucukRecipe.Id, ItemType = RecipeItemType.FatRawMaterial, ItemName = "Kuzu Kuyruk / Dana Kavram Yağı", PercentageRatio = 20.0m, QuantityPerBatchKg = 20.0m, StandardUnitCost = 180.0m, Description = "Lezzet ve nem koruyucu yağ tabakası (%20)" },
                    new() { ProcessingRecipeId = sucukRecipe.Id, ItemType = RecipeItemType.SpiceMix, ItemName = "Özel Sucuk Baharatı (Kimyon, Sarımsak, Kırmızı Biber)", PercentageRatio = 5.0m, QuantityPerBatchKg = 5.0m, StandardUnitCost = 150.0m, Description = "Doğal antimikrobiyal baharat karışımı" },
                    new() { ProcessingRecipeId = sucukRecipe.Id, ItemType = RecipeItemType.SaltAndCuring, ItemName = "Kaya Tuzu & Kürleme Tuzu", PercentageRatio = 2.0m, QuantityPerBatchKg = 2.0m, StandardUnitCost = 25.0m, Description = "Su aktivitesini düşürerek bozulmayı önler" },
                    new() { ProcessingRecipeId = sucukRecipe.Id, ItemType = RecipeItemType.CasingAndClip, ItemName = "Doğal Sığır Bağırsağı (Kangal) & Klips", PercentageRatio = 3.0m, QuantityPerBatchKg = 3.0m, StandardUnitCost = 90.0m, Description = "Doğal kurutma kılıfı" }
                };
                await context.ProcessingRecipeItems.AddRangeAsync(sucukItems);

                // 2. Özel Kasap Izgara Köfte (+6 Gün SKT)
                var kofteRecipe = new ProcessingRecipe
                {
                    RecipeCode = "RCP-KOFTE-01",
                    RecipeName = "Özel Baharatlı Kasap Köfte",
                    Category = RecipeProductCategory.MeatballsAndPatties,
                    StandardBatchWeightKg = 50.0m,
                    ExpectedYieldPercentage = 100.0m,
                    ShelfLifeExtensionDays = 6,
                    TargetProductName = "Kasap Köfte (Özel Baharatlı)",
                    Description = "Dana eti harmanlı, günlük taze kasap köftesi",
                    ProcessInstructions = "Dana kıyma orta yağlı çekilir. Köfte harcı, rendelenmiş soğan, maydanoz ve kaya tuzu ile 15 dk yoğrulur. Porsiyonlanıp dinlendirilir."
                };
                await context.ProcessingRecipes.AddAsync(kofteRecipe);
                await context.SaveChangesAsync();

                var kofteItems = new List<ProcessingRecipeItem>
                {
                    new() { ProcessingRecipeId = kofteRecipe.Id, ItemType = RecipeItemType.MeatRawMaterial, ItemName = "Dana Kıyma / Boşluk Eti", PercentageRatio = 80.0m, QuantityPerBatchKg = 40.0m, StandardUnitCost = 400.0m, Description = "Kurtarılan reyon kıyması (%80)" },
                    new() { ProcessingRecipeId = kofteRecipe.Id, ItemType = RecipeItemType.FatRawMaterial, ItemName = "Kavram Yağı", PercentageRatio = 8.0m, QuantityPerBatchKg = 4.0m, StandardUnitCost = 160.0m, Description = "Sulu köfte kıvamı için yağ" },
                    new() { ProcessingRecipeId = kofteRecipe.Id, ItemType = RecipeItemType.SpiceMix, ItemName = "Köfte Baharat Karışımı (Karabiber, Kimyon, Kekik)", PercentageRatio = 6.0m, QuantityPerBatchKg = 3.0m, StandardUnitCost = 90.0m, Description = "Geleneksel köfte harcı" },
                    new() { ProcessingRecipeId = kofteRecipe.Id, ItemType = RecipeItemType.SauceAndMarinade, ItemName = "Soğan Suyu & Sarımsak Özütü", PercentageRatio = 4.0m, QuantityPerBatchKg = 2.0m, StandardUnitCost = 45.0m, Description = "Doğal koruyucu ve yumuşatıcı" },
                    new() { ProcessingRecipeId = kofteRecipe.Id, ItemType = RecipeItemType.SaltAndCuring, ItemName = "Kaya Tuzu", PercentageRatio = 2.0m, QuantityPerBatchKg = 1.0m, StandardUnitCost = 20.0m, Description = "Kıvam bağlayıcı ve koruyucu tuz" }
                };
                await context.ProcessingRecipeItems.AddRangeAsync(kofteItems);

                // 3. Kekikli & Zeytinyağlı Soslu Dana Kuşbaşı (+10 Gün SKT, %108 Randıman)
                var sosluRecipe = new ProcessingRecipe
                {
                    RecipeCode = "RCP-SOSLU-01",
                    RecipeName = "Kekikli & Zeytinyağlı Soslu Dana Kuşbaşı",
                    Category = RecipeProductCategory.MarinatedPrimalCuts,
                    StandardBatchWeightKg = 50.0m,
                    ExpectedYieldPercentage = 108.0m, // Sos emilimi ile randıman artışı
                    ShelfLifeExtensionDays = 10,
                    TargetProductName = "Kekikli & Zeytinyağlı Soslu Dana Kuşbaşı",
                    Description = "Sızma zeytinyağı, elma sirkesi ve taze dağ kekiğiyle marine edilmiş yumuşak dana eti",
                    ProcessInstructions = "Kuşbaşı etler zeytinyağlı ve sirkeli marinat ile harmanlanır. Vakum tamburunda veya soğuk odada 12 saat marine edilir. Asit ve yağ et yüzeyinde koruyucu bariyer oluşturur."
                };
                await context.ProcessingRecipes.AddAsync(sosluRecipe);
                await context.SaveChangesAsync();

                var sosluItems = new List<ProcessingRecipeItem>
                {
                    new() { ProcessingRecipeId = sosluRecipe.Id, ItemType = RecipeItemType.MeatRawMaterial, ItemName = "Dana Kuşbaşı / Tranç Parça Et", PercentageRatio = 84.0m, QuantityPerBatchKg = 42.0m, StandardUnitCost = 420.0m, Description = "Reyondan çekilen taze dana kuşbaşı" },
                    new() { ProcessingRecipeId = sosluRecipe.Id, ItemType = RecipeItemType.SauceAndMarinade, ItemName = "Sızma Zeytinyağı & Elma Sirkesi Marinadı", PercentageRatio = 10.0m, QuantityPerBatchKg = 5.0m, StandardUnitCost = 140.0m, Description = "Yüzey pH'ını düşürerek raf ömrünü uzatır" },
                    new() { ProcessingRecipeId = sosluRecipe.Id, ItemType = RecipeItemType.SpiceMix, ItemName = "Dağ Kekiği, Biberiye & Tane Karabiber", PercentageRatio = 4.0m, QuantityPerBatchKg = 2.0m, StandardUnitCost = 120.0m, Description = "Güçlü antioksidan ve antimikrobiyal etki" },
                    new() { ProcessingRecipeId = sosluRecipe.Id, ItemType = RecipeItemType.SaltAndCuring, ItemName = "Kaya Tuzu & Sarımsak Püresi", PercentageRatio = 2.0m, QuantityPerBatchKg = 1.0m, StandardUnitCost = 35.0m, Description = "Tatlandırıcı ve koruyucu" }
                };
                await context.ProcessingRecipeItems.AddRangeAsync(sosluItems);

                // 4. Barbekü & Biberiyeli Marine Dana Biftek (+12 Gün SKT, %106 Randıman)
                var bbqRecipe = new ProcessingRecipe
                {
                    RecipeCode = "RCP-SOSLU-02",
                    RecipeName = "Füme Barbekü & Biberiyeli Marine Dana Biftek",
                    Category = RecipeProductCategory.MarinatedPrimalCuts,
                    StandardBatchWeightKg = 40.0m,
                    ExpectedYieldPercentage = 106.0m,
                    ShelfLifeExtensionDays = 12,
                    TargetProductName = "Barbekü Soslu Marine Dana Biftek",
                    Description = "Özel tütsülü barbekü sosu ve taze biberiyeyle marine edilmiş gurme dana biftek",
                    ProcessInstructions = "Biftek dilimleri barbekü marinatına yatırılır. +2°C'de 8 saat dinlendirildikten sonra MAP tabaklara yerleştirilir."
                };
                await context.ProcessingRecipes.AddAsync(bbqRecipe);
                await context.SaveChangesAsync();

                var bbqItems = new List<ProcessingRecipeItem>
                {
                    new() { ProcessingRecipeId = bbqRecipe.Id, ItemType = RecipeItemType.MeatRawMaterial, ItemName = "Dana Antrikot / Kontrfile Dilimleri", PercentageRatio = 85.0m, QuantityPerBatchKg = 34.0m, StandardUnitCost = 520.0m, Description = "Dilimlenmiş asil biftek eti" },
                    new() { ProcessingRecipeId = bbqRecipe.Id, ItemType = RecipeItemType.SauceAndMarinade, ItemName = "Füme Barbekü & Worcestershire Sos", PercentageRatio = 9.0m, QuantityPerBatchKg = 3.6m, StandardUnitCost = 110.0m, Description = "Karamelize tütsü lezzeti ve asit dengesi" },
                    new() { ProcessingRecipeId = bbqRecipe.Id, ItemType = RecipeItemType.SpiceMix, ItemName = "Taze Biberiye, Sarımsak & Füme Pul Biber", PercentageRatio = 4.0m, QuantityPerBatchKg = 1.6m, StandardUnitCost = 130.0m, Description = "Baharat ve aroma harmanı" },
                    new() { ProcessingRecipeId = bbqRecipe.Id, ItemType = RecipeItemType.SaltAndCuring, ItemName = "Deniz Tuzu", PercentageRatio = 2.0m, QuantityPerBatchKg = 0.8m, StandardUnitCost = 30.0m, Description = "Nem tutucu ve mineral dengesi" }
                };
                await context.ProcessingRecipeItems.AddRangeAsync(bbqItems);
                await context.SaveChangesAsync();
            }

            // 5. SEED INDUSTRIAL BUTCHERY EQUIPMENT PARK
            var equipmentCount = await context.MeatProcessingEquipments.CountAsync();
            if (equipmentCount == 0)
            {
                var equipments = new List<MeatProcessingEquipment>
                {
                    new()
                    {
                        EquipmentCode = "EQP-KYM-01",
                        EquipmentName = "Soğutmalı Kasap Kıyma Makinesi (No: 32)",
                        EquipmentType = "Kıyma Makinesi",
                        CapacityInfo = "600 kg/saat",
                        OperatingStatus = "Hazır / Operasyonel",
                        LastSanitizationDate = DateTime.UtcNow.AddHours(-2),
                        SanitizedBy = "Mustafa Usta",
                        SanitizingAgent = "Perasetik Asit (%0.2) + 82°C Sıcak Su",
                        IsSanitizedAndReady = true,
                        NextMaintenanceDate = DateTime.UtcNow.AddMonths(1),
                        Notes = "Çift ayna, sinir ayırıcılı endüstriyel paslanmaz çelik bıçaklar."
                    },
                    new()
                    {
                        EquipmentCode = "EQP-MAR-01",
                        EquipmentName = "Endüstriyel Vakumlu Marinasyon Tamburu",
                        EquipmentType = "Vakum Marinatör",
                        CapacityInfo = "120 kg/parti",
                        OperatingStatus = "Hazır / Operasyonel",
                        LastSanitizationDate = DateTime.UtcNow.AddHours(-1),
                        SanitizedBy = "Ahmet Kalfa",
                        SanitizingAgent = "Klor Dioksit Köpük + Durulama",
                        IsSanitizedAndReady = true,
                        NextMaintenanceDate = DateTime.UtcNow.AddMonths(2),
                        Notes = "Negatif basınçlı derin lif penetrasyonu ile hızlı sos çekimi."
                    },
                    new()
                    {
                        EquipmentCode = "EQP-DOL-01",
                        EquipmentName = "Hidrolik Pistonlu Sucuk/Sosis Dolum Makinesi",
                        EquipmentType = "Dolum & Porsiyonlama",
                        CapacityInfo = "25 L / Parti",
                        OperatingStatus = "Hazır / Operasyonel",
                        LastSanitizationDate = DateTime.UtcNow.AddHours(-4),
                        SanitizedBy = "Mustafa Usta",
                        SanitizingAgent = "Alkol Bazlı Yüzey Dezenfektanı",
                        IsSanitizedAndReady = true,
                        NextMaintenanceDate = DateTime.UtcNow.AddMonths(1),
                        Notes = "Hava tahliyeli huni, doğal ve kolajen kılıf uyumlu hassas piston."
                    },
                    new()
                    {
                        EquipmentCode = "EQP-YOG-01",
                        EquipmentName = "Paslanmaz Çift Kollu Köfte Yoğurma Kazanı",
                        EquipmentType = "Yoğurucu & Mikser",
                        CapacityInfo = "80 kg/parti",
                        OperatingStatus = "Hazır / Operasyonel",
                        LastSanitizationDate = DateTime.UtcNow.AddHours(-3),
                        SanitizedBy = "Mehmet Usta",
                        SanitizingAgent = "Sıcak Su + Alkali Köpük",
                        IsSanitizedAndReady = true,
                        NextMaintenanceDate = DateTime.UtcNow.AddMonths(2),
                        Notes = "Homojen baharat dağılımı, eti ezmeden yoğuran helisel kollar."
                    },
                    new()
                    {
                        EquipmentCode = "EQP-STR-01",
                        EquipmentName = "UV-C Bıçak & Satır Sterilizasyon Dolabı",
                        EquipmentType = "Sterilizatör",
                        CapacityInfo = "30 Bıçak Kapasiteli",
                        OperatingStatus = "Hazır / Operasyonel",
                        LastSanitizationDate = DateTime.UtcNow.AddMinutes(-30),
                        SanitizedBy = "Nöbetçi Personel",
                        SanitizingAgent = "254nm UV-C Germisidal Işıma",
                        IsSanitizedAndReady = true,
                        NextMaintenanceDate = DateTime.UtcNow.AddMonths(3),
                        Notes = "Çapraz bulaşmayı %99.99 engelleyen steril saklama kabini."
                    }
                };
                await context.MeatProcessingEquipments.AddRangeAsync(equipments);
                await context.SaveChangesAsync();
            }

            // 6. SEED AUXILIARY MATERIAL & SPICE/SAUCE INVENTORY
            var auxCount = await context.AuxiliaryMaterialStocks.CountAsync();
            if (auxCount == 0)
            {
                var auxMaterials = new List<AuxiliaryMaterialStock>
                {
                    new()
                    {
                        MaterialCode = "BAH-SCK-01",
                        MaterialName = "Geleneksel Sucuk Baharat Harcı (Kimyon, Sarımsak, Biber)",
                        ItemType = RecipeItemType.SpiceMix,
                        CurrentStock = 45.0m,
                        UnitOfMeasure = "KG",
                        UnitCost = 150.0m,
                        MinStockLevel = 10.0m,
                        ExpiryDate = DateTime.UtcNow.AddMonths(10),
                        StorageCondition = "Kuru & Serin (+15°C)"
                    },
                    new()
                    {
                        MaterialCode = "BAH-KFT-01",
                        MaterialName = "Özel Kasap Köfte Baharat Miksi",
                        ItemType = RecipeItemType.SpiceMix,
                        CurrentStock = 38.0m,
                        UnitOfMeasure = "KG",
                        UnitCost = 90.0m,
                        MinStockLevel = 10.0m,
                        ExpiryDate = DateTime.UtcNow.AddMonths(8),
                        StorageCondition = "Kuru & Serin (+15°C)"
                    },
                    new()
                    {
                        MaterialCode = "SOS-MRN-01",
                        MaterialName = "Zeytinyağlı & Kekikli Marinasyon Sosu",
                        ItemType = RecipeItemType.SauceAndMarinade,
                        CurrentStock = 28.0m,
                        UnitOfMeasure = "KG",
                        UnitCost = 140.0m,
                        MinStockLevel = 8.0m,
                        ExpiryDate = DateTime.UtcNow.AddMonths(4),
                        StorageCondition = "Soğuk Depo (+4°C)"
                    },
                    new()
                    {
                        MaterialCode = "SOS-BBQ-01",
                        MaterialName = "Füme Barbekü & Biberiye Sosu",
                        ItemType = RecipeItemType.SauceAndMarinade,
                        CurrentStock = 22.0m,
                        UnitOfMeasure = "KG",
                        UnitCost = 110.0m,
                        MinStockLevel = 5.0m,
                        ExpiryDate = DateTime.UtcNow.AddMonths(5),
                        StorageCondition = "Soğuk Depo (+4°C)"
                    },
                    new()
                    {
                        MaterialCode = "BGR-DGL-01",
                        MaterialName = "Doğal Sığır Kangal Bağırsağı (Kuru Tuzlu)",
                        ItemType = RecipeItemType.CasingAndClip,
                        CurrentStock = 180.0m,
                        UnitOfMeasure = "MT",
                        UnitCost = 18.0m,
                        MinStockLevel = 30.0m,
                        ExpiryDate = DateTime.UtcNow.AddMonths(12),
                        StorageCondition = "Tuz Salamura (+4°C)"
                    },
                    new()
                    {
                        MaterialCode = "TUZ-KYA-01",
                        MaterialName = "Çankırı Doğal Kaya Tuzu (İnce Çekim)",
                        ItemType = RecipeItemType.SaltAndCuring,
                        CurrentStock = 120.0m,
                        UnitOfMeasure = "KG",
                        UnitCost = 20.0m,
                        MinStockLevel = 25.0m,
                        ExpiryDate = DateTime.UtcNow.AddYears(2),
                        StorageCondition = "Kuru Ambar"
                    },
                    new()
                    {
                        MaterialCode = "AMB-MAP-01",
                        MaterialName = "Gazlı MAP Tabak (Modifiye Atmosfer 500g)",
                        ItemType = RecipeItemType.PackagingMaterial,
                        CurrentStock = 650.0m,
                        UnitOfMeasure = "ADT",
                        UnitCost = 3.50m,
                        MinStockLevel = 100.0m,
                        ExpiryDate = DateTime.UtcNow.AddYears(3),
                        StorageCondition = "Kuru Ambar"
                    },
                    new()
                    {
                        MaterialCode = "AMB-VKM-01",
                        MaterialName = "9 Katmanlı Yüksek Bariyerli Vakum Poşeti (20x30)",
                        ItemType = RecipeItemType.PackagingMaterial,
                        CurrentStock = 850.0m,
                        UnitOfMeasure = "ADT",
                        UnitCost = 2.20m,
                        MinStockLevel = 150.0m,
                        ExpiryDate = DateTime.UtcNow.AddYears(3),
                        StorageCondition = "Kuru Ambar"
                    }
                };
                await context.AuxiliaryMaterialStocks.AddRangeAsync(auxMaterials);
                await context.SaveChangesAsync();
            }

            // 7. SEED FOOD SAFETY & QUALITY CCP INSPECTION RECORDS
            var qcCount = await context.ReprocessingQualityChecks.CountAsync();
            if (qcCount == 0)
            {
                var qcChecks = new List<ReprocessingQualityCheck>
                {
                    new()
                    {
                        InspectionNumber = "QC-20260912-001",
                        BatchLotNumber = "LOT-2026-DNK-01",
                        ProductName = "Dana Kuşbaşı",
                        MeasuredPh = 5.62m,
                        MeasuredWaterActivityAw = 0.88m,
                        MeasuredCoreTempCelsius = 2.8m,
                        QualityVerdict = "✅ Onaylandı (Güvenli Dönüşüm)",
                        IsApproved = true,
                        IsAutoBlocked = false,
                        InspectorName = "Dr. Vet. Mehmet Demir",
                        Notes = "Taze et kokusu ve dokusu mükemmel, mikrobiyal yük sınır altında.",
                        InspectionDate = DateTime.UtcNow.AddHours(-18)
                    },
                    new()
                    {
                        InspectionNumber = "QC-20260912-002",
                        BatchLotNumber = "LOT-2026-KYM-02",
                        ProductName = "Dana Kıyma",
                        MeasuredPh = 5.75m,
                        MeasuredWaterActivityAw = 0.89m,
                        MeasuredCoreTempCelsius = 3.4m,
                        QualityVerdict = "✅ Onaylandı (Güvenli Dönüşüm)",
                        IsApproved = true,
                        IsAutoBlocked = false,
                        InspectorName = "Dr. Vet. Mehmet Demir",
                        Notes = "Köfte harcına uygun, renk parlak, asitlik dengeli.",
                        InspectionDate = DateTime.UtcNow.AddHours(-10)
                    },
                    new()
                    {
                        InspectionNumber = "QC-20260911-003",
                        BatchLotNumber = "LOT-2026-TAV-99",
                        ProductName = "Tavuk But (İmha Örneği)",
                        MeasuredPh = 6.45m,
                        MeasuredWaterActivityAw = 0.95m,
                        MeasuredCoreTempCelsius = 9.1m,
                        QualityVerdict = "⛔ Bloke Edildi (Kritik pH > 6.20 Limiti Aşıldı - Bozulma Riski)",
                        IsApproved = false,
                        IsAutoBlocked = true,
                        InspectorName = "Gıda Müh. Ayşe Kaya",
                        Notes = "Kokuşma başlangıcı ve yüksek sıcaklık tespiti yapıldı, derhal imha konteynerine sevk edildi.",
                        InspectionDate = DateTime.UtcNow.AddDays(-1)
                    }
                };
                await context.ReprocessingQualityChecks.AddRangeAsync(qcChecks);
                await context.SaveChangesAsync();
            }

            // 8. SEED SUCUK CURING & FERMENTATION CHAMBERS
            var curingCount = await context.SucukCuringBatches.CountAsync();
            if (curingCount == 0)
            {
                var curingBatches = new List<SucukCuringBatch>
                {
                    new()
                    {
                        CuringLotNumber = "CUR-2026-SCK-01",
                        RecipeName = "Geleneksel Fermente Kasap Sucuğu",
                        ChamberRoomCode = "ODA-FERM-01",
                        InitialGreenWeightKg = 100.0m,
                        CurrentWeightKg = 91.5m,
                        TargetDryWeightKg = 88.0m,
                        InitialPh = 5.65m,
                        CurrentPh = 5.15m,
                        ChamberTemperatureCelsius = 15.2m,
                        ChamberHumidityRh = 78.5m,
                        ElapsedDays = 5,
                        TargetCuringDays = 10,
                        Status = "Olgunlaşıyor (Fermantasyonda)",
                        ResponsibleButcher = "Mustafa Usta",
                        Notes = "Laktik asit fermantasyonu stabil ilerliyor, renk koyulaşması ve aromatik koku gelişimi ideal."
                    },
                    new()
                    {
                        CuringLotNumber = "CUR-2026-SCK-02",
                        RecipeName = "Acılı Osmanlı Kangal Sucuğu",
                        ChamberRoomCode = "ODA-FERM-02",
                        InitialGreenWeightKg = 80.0m,
                        CurrentWeightKg = 70.4m,
                        TargetDryWeightKg = 70.4m,
                        InitialPh = 5.60m,
                        CurrentPh = 5.02m,
                        ChamberTemperatureCelsius = 14.8m,
                        ChamberHumidityRh = 76.0m,
                        ElapsedDays = 10,
                        TargetCuringDays = 10,
                        Status = "✓ Tamamlandı (Satışa Hazır)",
                        ResponsibleButcher = "Mustafa Usta",
                        Notes = "%12 kuruma firesi tamamlandı. Sertlik ve pH mükemmel, reyon satışına ve vakumlamaya sevk edilebilir."
                    }
                };
                await context.SucukCuringBatches.AddRangeAsync(curingBatches);
                await context.SaveChangesAsync();
            }

            // 9. SEED OFFICIAL DISPOSAL & RENDERING RECORDS
            var disposalCount = await context.ReprocessingDisposalRecords.CountAsync();
            if (disposalCount == 0)
            {
                var disposalRecords = new List<ReprocessingDisposalRecord>
                {
                    new()
                    {
                        ProtocolNumber = "IMHA-2026-0912-001",
                        BatchLotNumber = "LOT-2026-TAV-99",
                        ProductName = "Tavuk But (Bozulmuş)",
                        DisposedQuantityKg = 18.5m,
                        EstimatedFinancialLossLira = 2775.00m,
                        DisposalReason = "Kritik pH > 6.20 ve Yüksek Sıcaklık Tespiti (Bozulma)",
                        MeasuredPh = 6.45m,
                        SensoryFailureNotes = "Ekşi koku ve yapışkan yüzey sıvısı, organoleptik ret.",
                        FirstApprover = "Mustafa Usta (Reyon / Kasap Şefi)",
                        SecondApprover = "Dr. Vet. Mehmet Demir (HACCP Sorumlusu)",
                        ApprovalDate = DateTime.UtcNow.AddDays(-1),
                        RenderingCompanyName = "Eko-Bertaraf & Biyo-Enerji A.Ş. (Bakanlık Lisanslı)",
                        WaybillNumber = "IRS-2026-44211",
                        DisposalStatus = "Lisanslı Rendering İmhaya Teslim Edildi"
                    }
                };
                await context.ReprocessingDisposalRecords.AddRangeAsync(disposalRecords);
                await context.SaveChangesAsync();
            }

            // 10. SEED BUTCHER WASTE PREVENTION INCENTIVES
            var incentiveCount = await context.ButcherWasteIncentives.CountAsync();
            if (incentiveCount == 0)
            {
                var incentives = new List<ButcherWasteIncentive>
                {
                    new()
                    {
                        ButcherName = "Mustafa Usta",
                        RoleTitle = "Usta Kasap / Şarküteri Şefi",
                        PeriodMonth = "Eylül 2026",
                        TotalRescuedMeatKg = 145.5m,
                        TotalNetValueCreatedLira = 32450.0m,
                        IncentiveRatePercentage = 2.0m,
                        TotalBatchesReprocessed = 8,
                        BadgeTitle = "🥇 Altın Kasap (Sıfır Fire Şampiyonu)"
                    },
                    new()
                    {
                        ButcherName = "Ahmet Kalfa",
                        RoleTitle = "Şarküteri & Marinasyon Sorumlusu",
                        PeriodMonth = "Eylül 2026",
                        TotalRescuedMeatKg = 85.0m,
                        TotalNetValueCreatedLira = 17800.0m,
                        IncentiveRatePercentage = 1.5m,
                        TotalBatchesReprocessed = 5,
                        BadgeTitle = "🥈 Gümüş Kasap (Marinasyon Ustası)"
                    }
                };
                await context.ButcherWasteIncentives.AddRangeAsync(incentives);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Mezbaha tabloları veya dünya ırkları kataloğu yüklenirken hata oluştu.");
        }
    }
}

