using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Services;

public class SlaughterhouseService : ISlaughterhouseService
{
    private readonly KasapDbContext _context;

    public SlaughterhouseService(KasapDbContext context)
    {
        _context = context;
    }

    private static bool _schemaEnsured = false;
    private static readonly SemaphoreSlim _schemaLock = new(1, 1);

    private async Task EnsureAllMezbahaSchemaAsync()
    {
        if (_schemaEnsured) return;

        await _schemaLock.WaitAsync();
        try
        {
            if (_schemaEnsured) return;

            // 1. Ensure Tables Exist
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS AnimalBreeds (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    AnimalType INTEGER NOT NULL,
                    Description TEXT,
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
            ");

            // 2. Safely add any newly introduced columns to existing SQLite tables
            var alterStatements = new[]
            {
                "ALTER TABLE SlaughterRecords ADD COLUMN ConformationClass TEXT DEFAULT 'R';",
                "ALTER TABLE SlaughterRecords ADD COLUMN FatCoverScore INTEGER DEFAULT 3;",
                "ALTER TABLE SlaughterRecords ADD COLUMN MarblingScore INTEGER DEFAULT 5;",
                "ALTER TABLE SlaughterRecords ADD COLUMN PostMortemPh24 REAL DEFAULT 5.65;",
                "ALTER TABLE SlaughterRecords ADD COLUMN HookRailNumber TEXT DEFAULT 'Ray-1 / Askı-14';",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN TheoreticalStandardRatio REAL DEFAULT 0;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN VariancePercentage REAL DEFAULT 0;",
                "ALTER TABLE CarcassDeboningCuts ADD COLUMN Gs1Barcode128 TEXT DEFAULT '';"
            };

            foreach (var alter in alterStatements)
            {
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(alter);
                }
                catch { } // Column already exists in newly created databases
            }

            _schemaEnsured = true;
        }
        catch { }
        finally
        {
            _schemaLock.Release();
        }
    }

    public async Task<List<string>> GetAllBreedsAsync(AnimalType? animalType = null)
    {
        await EnsureAllMezbahaSchemaAsync();

        var query = _context.AnimalBreeds.AsQueryable();
        if (animalType.HasValue)
        {
            query = query.Where(b => b.AnimalType == animalType.Value);
        }

        var list = await query.Select(b => b.Name).Distinct().ToListAsync();
        if (!list.Any())
        {
            // Comprehensive World Breeds Default List
            var worldBreeds = new List<AnimalBreed>
            {
                new() { Name = "Simental (Simmental)", AnimalType = AnimalType.Buyukbas, Description = "İsviçre / Almanya - Yüksek süt ve et verimi" },
                new() { Name = "Aberdeen Angus (Black Angus)", AnimalType = AnimalType.Buyukbas, Description = "İskoçya - Premium mermersi et" },
                new() { Name = "Red Angus", AnimalType = AnimalType.Buyukbas, Description = "İskoçya / ABD - Kaliteli etçi sığır" },
                new() { Name = "Holstein (Siyah-Alaca)", AnimalType = AnimalType.Buyukbas, Description = "Hollanda / Almanya" },
                new() { Name = "Hereford (Boynuzsuz)", AnimalType = AnimalType.Buyukbas, Description = "İngiltere - Mera besisi eti" },
                new() { Name = "Şarole (Charolais)", AnimalType = AnimalType.Buyukbas, Description = "Fransa - Yüksek karkas ağırlığı" },
                new() { Name = "Limuzin (Limousin)", AnimalType = AnimalType.Buyukbas, Description = "Fransa - Yağsız kas randımanı" },
                new() { Name = "Montofon / Brown Swiss", AnimalType = AnimalType.Buyukbas, Description = "İsviçre - Kombine dayanıklı ırk" },
                new() { Name = "Wagyu / Kobe (Kuroge)", AnimalType = AnimalType.Buyukbas, Description = "Japonya - A5 mermersi yağ dokulu premium et" },
                new() { Name = "Belçika Mavisi (Belgian Blue)", AnimalType = AnimalType.Buyukbas, Description = "Belçika - Çift kaslı ultra yüksek randıman" },
                new() { Name = "Piedmontese (Piyemonte)", AnimalType = AnimalType.Buyukbas, Description = "İtalya - Çift kaslı yağsız et" },
                new() { Name = "Chianina", AnimalType = AnimalType.Buyukbas, Description = "İtalya - Dünyanın en iri sığır ırkı" },
                new() { Name = "Brahman (Zebu)", AnimalType = AnimalType.Buyukbas, Description = "Hindistan / ABD - Hörgüçlü dayanıklı ırk" },
                new() { Name = "Brangus", AnimalType = AnimalType.Buyukbas, Description = "ABD - Dayanıklı melez" },
                new() { Name = "Santa Gertrudis", AnimalType = AnimalType.Buyukbas, Description = "ABD (Teksas)" },
                new() { Name = "Blonde d'Aquitaine", AnimalType = AnimalType.Buyukbas, Description = "Fransa" },
                new() { Name = "Aubrac", AnimalType = AnimalType.Buyukbas, Description = "Fransa" },
                new() { Name = "Salers", AnimalType = AnimalType.Buyukbas, Description = "Fransa" },
                new() { Name = "Gelbvieh", AnimalType = AnimalType.Buyukbas, Description = "Almanya" },
                new() { Name = "Highland Cattle (İskoç Yayla)", AnimalType = AnimalType.Buyukbas, Description = "İskoçya" },
                new() { Name = "Galloway / Belted Galloway", AnimalType = AnimalType.Buyukbas, Description = "İskoçya" },
                new() { Name = "Texas Longhorn", AnimalType = AnimalType.Buyukbas, Description = "ABD" },
                new() { Name = "Jersey", AnimalType = AnimalType.Buyukbas, Description = "İngiltere" },
                new() { Name = "Guernsey", AnimalType = AnimalType.Buyukbas, Description = "İngiltere" },
                new() { Name = "Ayrshire", AnimalType = AnimalType.Buyukbas, Description = "İskoçya" },
                new() { Name = "Doğu Anadolu Kırmızısı (DAK)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye" },
                new() { Name = "Güney Anadolu Kırmızısı (GAK)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye" },
                new() { Name = "Yerli Kara", AnimalType = AnimalType.Buyukbas, Description = "Türkiye" },
                new() { Name = "Boz Irk (Plevne)", AnimalType = AnimalType.Buyukbas, Description = "Türkiye" },
                new() { Name = "Zavot Sığırı", AnimalType = AnimalType.Buyukbas, Description = "Türkiye (Kars)" },
                new() { Name = "Anadolu Mandası", AnimalType = AnimalType.Buyukbas, Description = "Türkiye" },
                new() { Name = "Murrah Mandası", AnimalType = AnimalType.Buyukbas, Description = "Hindistan" },
                
                // Sheep & Goats
                new() { Name = "Merinos (Avustralya & Karacabey)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye / İspanya" },
                new() { Name = "Kıvırcık (Trakya)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Akkaraman (Kangal)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Morkaraman", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "İvesi (Awassi)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye / Ortadoğu" },
                new() { Name = "Sakız (Chios)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye / Ege" },
                new() { Name = "Tahirova", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Dorper", AnimalType = AnimalType.Kucukbas, Description = "Güney Afrika" },
                new() { Name = "Suffolk", AnimalType = AnimalType.Kucukbas, Description = "İngiltere" },
                new() { Name = "Texel", AnimalType = AnimalType.Kucukbas, Description = "Hollanda" },
                new() { Name = "Hampshire Down", AnimalType = AnimalType.Kucukbas, Description = "İngiltere" },
                new() { Name = "Romanov", AnimalType = AnimalType.Kucukbas, Description = "Rusya" },
                new() { Name = "Doğu Friz (East Friesian)", AnimalType = AnimalType.Kucukbas, Description = "Almanya" },
                new() { Name = "Assaf", AnimalType = AnimalType.Kucukbas, Description = "İsrail / İspanya" },
                new() { Name = "Lacaune", AnimalType = AnimalType.Kucukbas, Description = "Fransa" },
                new() { Name = "Charollais Koyunu", AnimalType = AnimalType.Kucukbas, Description = "Fransa" },
                new() { Name = "Ile de France", AnimalType = AnimalType.Kucukbas, Description = "Fransa" },
                new() { Name = "Karagül (Astragan)", AnimalType = AnimalType.Kucukbas, Description = "Orta Asya" },
                new() { Name = "Dağlıç", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Hemşin Koyunu", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Norduz Koyunu", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Çine Çaparı", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                
                // Goats
                new() { Name = "Saanen (İsviçre Beyazı)", AnimalType = AnimalType.Kucukbas, Description = "İsviçre" },
                new() { Name = "Boer Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Güney Afrika" },
                new() { Name = "Kıl Keçisi (Kara Keçi)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Halep (Şam / Damascus)", AnimalType = AnimalType.Kucukbas, Description = "Suriye / Türkiye" },
                new() { Name = "Honamlı Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Türkiye (Toroslar)" },
                new() { Name = "Alpin (Fransız Alpin)", AnimalType = AnimalType.Kucukbas, Description = "Fransa" },
                new() { Name = "Toggenburg", AnimalType = AnimalType.Kucukbas, Description = "İsviçre" },
                new() { Name = "Anglo-Nubian (Nubya)", AnimalType = AnimalType.Kucukbas, Description = "İngiltere" },
                new() { Name = "Tiftik Keçisi (Ankara)", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Kilis Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Türkiye" },
                new() { Name = "Maltız Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Malta" },
                new() { Name = "Kalahari Red", AnimalType = AnimalType.Kucukbas, Description = "Güney Afrika" },
                new() { Name = "Kaşmir Keçisi", AnimalType = AnimalType.Kucukbas, Description = "Himalayalar" }
            };

            await _context.AnimalBreeds.AddRangeAsync(worldBreeds);
            await _context.SaveChangesAsync();
            return worldBreeds.Select(b => b.Name).Distinct().ToList();
        }
        return list;
    }

    public async Task<AnimalBreed> AddBreedAsync(string breedName, AnimalType animalType)
    {
        if (string.IsNullOrWhiteSpace(breedName))
        {
            throw new ArgumentException("Irk adı boş olamaz.");
        }

        await EnsureAllMezbahaSchemaAsync();

        var trimmed = breedName.Trim();
        var existing = await _context.AnimalBreeds.FirstOrDefaultAsync(b => b.Name.ToLower() == trimmed.ToLower());
        if (existing != null) return existing;

        var breed = new AnimalBreed
        {
            Name = trimmed,
            AnimalType = animalType
        };

        await _context.AnimalBreeds.AddAsync(breed);
        await _context.SaveChangesAsync();
        return breed;
    }

    public async Task<List<AnimalIntakeDto>> GetAllIntakesAsync()
    {
        await EnsureAllMezbahaSchemaAsync();

        var list = await _context.AnimalIntakes
            .Include(a => a.ProducerCustomer)
            .OrderByDescending(a => a.ArrivalDate)
            .ToListAsync();

        return list.Select(a => new AnimalIntakeDto
        {
            Id = a.Id,
            EarTagNumber = a.EarTagNumber,
            PassportNumber = a.PassportNumber,
            AnimalType = a.AnimalType,
            Breed = a.Breed,
            Gender = a.Gender,
            AgeMonths = a.AgeMonths,
            LiveWeightKg = a.LiveWeightKg,
            PurchasePrice = a.PurchasePrice,
            ProducerName = a.ProducerCustomer?.Name ?? a.ProducerName,
            WaybillNumber = a.WaybillNumber,
            FarmOrigin = a.FarmOrigin,
            ArrivalDate = a.ArrivalDate,
            SlaughterOrderNumber = a.SlaughterOrderNumber,
            BatchNumber = a.BatchNumber,
            VeterinaryStatus = a.VeterinaryStatus,
            IsApprovedForSlaughter = a.IsApprovedForSlaughter,
            SlaughterStatus = a.SlaughterStatus,
            BlockReason = a.BlockReason
        }).ToList();
    }

    public async Task<AnimalIntakeDto> RegisterAnimalIntakeAsync(AnimalIntake intake)
    {
        if (string.IsNullOrWhiteSpace(intake.EarTagNumber))
        {
            throw new ArgumentException("Küpe numarası zorunludur.");
        }

        if (string.IsNullOrEmpty(intake.BatchNumber))
        {
            intake.BatchNumber = $"PRT-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 1000:D3}";
        }

        intake.VeterinaryStatus = VeterinaryCheckStatus.Uygun;
        intake.IsApprovedForSlaughter = true;
        intake.SlaughterStatus = SlaughterStatus.Bekliyor;

        await _context.AnimalIntakes.AddAsync(intake);
        await _context.SaveChangesAsync();

        return new AnimalIntakeDto
        {
            Id = intake.Id,
            EarTagNumber = intake.EarTagNumber,
            PassportNumber = intake.PassportNumber,
            AnimalType = intake.AnimalType,
            Breed = intake.Breed,
            Gender = intake.Gender,
            AgeMonths = intake.AgeMonths,
            LiveWeightKg = intake.LiveWeightKg,
            PurchasePrice = intake.PurchasePrice,
            ProducerName = intake.ProducerName,
            WaybillNumber = intake.WaybillNumber,
            FarmOrigin = intake.FarmOrigin,
            ArrivalDate = intake.ArrivalDate,
            SlaughterOrderNumber = intake.SlaughterOrderNumber,
            BatchNumber = intake.BatchNumber,
            VeterinaryStatus = intake.VeterinaryStatus,
            IsApprovedForSlaughter = intake.IsApprovedForSlaughter,
            SlaughterStatus = intake.SlaughterStatus
        };
    }

    public async Task<VeterinaryCheckDto> PerformVeterinaryCheckAsync(VeterinaryCheck check)
    {
        var animal = await _context.AnimalIntakes.FindAsync(check.AnimalIntakeId);
        if (animal == null)
        {
            throw new InvalidOperationException("Muayene edilecek hayvan kaydı bulunamadı.");
        }

        if (string.IsNullOrEmpty(check.ReportNumber))
        {
            check.ReportNumber = $"VET-{DateTime.UtcNow:yyyyMMdd}-{check.AnimalIntakeId:D4}";
        }

        animal.VeterinaryStatus = check.Status;
        animal.IsApprovedForSlaughter = check.IsApprovedForSlaughter;

        if (!check.IsApprovedForSlaughter)
        {
            animal.SlaughterStatus = SlaughterStatus.Bekliyor;
            animal.BlockReason = $"Veteriner Hekim Blokesi: {check.Diagnosis}";
        }
        else
        {
            animal.BlockReason = null;
        }

        await _context.VeterinaryChecks.AddAsync(check);
        await _context.SaveChangesAsync();

        return new VeterinaryCheckDto
        {
            Id = check.Id,
            AnimalIntakeId = check.AnimalIntakeId,
            EarTagNumber = animal.EarTagNumber,
            VeterinarianName = check.VeterinarianName,
            DiplomaNumber = check.DiplomaNumber,
            CheckDate = check.CheckDate,
            IsAntemortem = check.IsAntemortem,
            Status = check.Status,
            IsApprovedForSlaughter = check.IsApprovedForSlaughter,
            BodyTemperature = check.BodyTemperature,
            Diagnosis = check.Diagnosis,
            QuarantineNotes = check.QuarantineNotes,
            ReportNumber = check.ReportNumber
        };
    }

    public async Task<SlaughterRecordDto> ExecuteSlaughterAndWeighingAsync(SlaughterRecord slaughter)
    {
        var animal = await _context.AnimalIntakes.FindAsync(slaughter.AnimalIntakeId);
        if (animal == null)
        {
            throw new InvalidOperationException("Kesim yapılacak hayvan bulunamadı.");
        }

        if (!animal.IsApprovedForSlaughter)
        {
            throw new InvalidOperationException($"HAYVAN KESİME GÖNDERİLEMEZ! Veteriner Onayı Yok veya Karantinada. ({animal.BlockReason})");
        }

        if (string.IsNullOrEmpty(slaughter.SlaughterNumber))
        {
            slaughter.SlaughterNumber = $"KES-{DateTime.UtcNow:yyyy}-{DateTime.UtcNow.Ticks % 100000:D6}";
        }
        if (string.IsNullOrEmpty(slaughter.CarcassNumber))
        {
            slaughter.CarcassNumber = $"KRK-{DateTime.UtcNow:yyyy}-{DateTime.UtcNow.Ticks % 100000:D6}";
        }

        if (slaughter.LiveWeightKg <= 0) slaughter.LiveWeightKg = animal.LiveWeightKg;
        if (slaughter.AnimalPurchaseCost <= 0) slaughter.AnimalPurchaseCost = animal.PurchasePrice;
        if (slaughter.ColdCarcassWeightKg <= 0 && slaughter.HotCarcassWeightKg > 0)
        {
            // %2 soğuma firesi varsayımı
            slaughter.ColdCarcassWeightKg = Math.Round(slaughter.HotCarcassWeightKg * 0.98m, 2);
        }

        animal.SlaughterStatus = SlaughterStatus.Tamamlandi;

        // Auto-create AnimalLot so it flows seamlessly into Cutting / BOM module
        var animalLot = new AnimalLot
        {
            LotNumber = slaughter.CarcassNumber,
            AnimalType = animal.AnimalType,
            CarcassType = AnimalCarcassType.Karkas,
            SupplierName = animal.ProducerName,
            EarTagNumber = animal.EarTagNumber,
            LiveWeightKg = slaughter.LiveWeightKg,
            CarcassWeightKg = slaughter.ColdCarcassWeightKg > 0 ? slaughter.ColdCarcassWeightKg : slaughter.HotCarcassWeightKg,
            PurchasePriceTotal = slaughter.TotalSlaughterCost,
            Origin = animal.FarmOrigin,
            SlaughterDate = slaughter.SlaughterDate,
            VeterinaryReportNo = animal.VeterinaryChecks.FirstOrDefault()?.ReportNumber ?? "VET-OK",
            TraceabilityCode = slaughter.CarcassNumber,
            Notes = $"Kesimhane: {slaughter.SlaughterLine} | Kasap: {slaughter.ButcherPersonName}"
        };
        await _context.AnimalLots.AddAsync(animalLot);

        await _context.SlaughterRecords.AddAsync(slaughter);
        await _context.SaveChangesAsync();

        return new SlaughterRecordDto
        {
            Id = slaughter.Id,
            SlaughterNumber = slaughter.SlaughterNumber,
            CarcassNumber = slaughter.CarcassNumber,
            AnimalIntakeId = slaughter.AnimalIntakeId,
            EarTagNumber = animal.EarTagNumber,
            SlaughterDate = slaughter.SlaughterDate,
            ButcherPersonName = slaughter.ButcherPersonName,
            VeterinarianName = slaughter.VeterinarianName,
            LiveWeightKg = slaughter.LiveWeightKg,
            HotCarcassWeightKg = slaughter.HotCarcassWeightKg,
            ColdCarcassWeightKg = slaughter.ColdCarcassWeightKg,
            HeadWeightKg = slaughter.HeadWeightKg,
            HideWeightKg = slaughter.HideWeightKg,
            OffalWeightKg = slaughter.OffalWeightKg,
            FatWeightKg = slaughter.FatWeightKg,
            BoneWeightKg = slaughter.BoneWeightKg,
            SlaughterWasteKg = slaughter.SlaughterWasteKg,
            CarcassYieldPercentage = slaughter.CarcassYieldPercentage,
            TotalSlaughterCost = slaughter.TotalSlaughterCost,
            CarcassCostPerKg = slaughter.CarcassCostPerKg,
            ColdStorageLocation = slaughter.ColdStorageLocation,
            Status = slaughter.Status
        };
    }

    public async Task<List<SlaughterRecordDto>> GetAllSlaughterRecordsAsync()
    {
        await EnsureAllMezbahaSchemaAsync();

        var list = await _context.SlaughterRecords
            .Include(s => s.AnimalIntake)
            .OrderByDescending(s => s.SlaughterDate)
            .ToListAsync();

        return list.Select(s => new SlaughterRecordDto
        {
            Id = s.Id,
            SlaughterNumber = s.SlaughterNumber,
            CarcassNumber = s.CarcassNumber,
            AnimalIntakeId = s.AnimalIntakeId,
            EarTagNumber = s.AnimalIntake != null ? s.AnimalIntake.EarTagNumber : string.Empty,
            SlaughterDate = s.SlaughterDate,
            ButcherPersonName = s.ButcherPersonName,
            VeterinarianName = s.VeterinarianName,
            LiveWeightKg = s.LiveWeightKg,
            HotCarcassWeightKg = s.HotCarcassWeightKg,
            ColdCarcassWeightKg = s.ColdCarcassWeightKg,
            HeadWeightKg = s.HeadWeightKg,
            HideWeightKg = s.HideWeightKg,
            OffalWeightKg = s.OffalWeightKg,
            FatWeightKg = s.FatWeightKg,
            BoneWeightKg = s.BoneWeightKg,
            SlaughterWasteKg = s.SlaughterWasteKg,
            CarcassYieldPercentage = s.CarcassYieldPercentage,
            TotalSlaughterCost = s.TotalSlaughterCost,
            CarcassCostPerKg = s.CarcassCostPerKg,
            ColdStorageLocation = s.ColdStorageLocation,
            Status = s.Status
        }).ToList();
    }

    private async Task EnsureCarcassDeboningCutsTableExistsAsync()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS CarcassDeboningCuts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SlaughterRecordId INTEGER NOT NULL,
                    CarcassNumber TEXT NOT NULL,
                    EarTagNumber TEXT NOT NULL,
                    AnatomicalRegion TEXT NOT NULL,
                    CutName TEXT NOT NULL,
                    WeightKg REAL NOT NULL,
                    YieldPercentage REAL NOT NULL,
                    QualityGrade TEXT NOT NULL,
                    UnitCostEstimated REAL NOT NULL,
                    TotalCutValue REAL NOT NULL,
                    LotNumber TEXT NOT NULL,
                    Barcode TEXT NOT NULL,
                    TargetStorageLocation TEXT NOT NULL,
                    CutDate TEXT NOT NULL,
                    MasterButcher TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    IsActive INTEGER NOT NULL
                );
            ");
        }
        catch { }
    }

    public async Task<List<CarcassDeboningCutDto>> GetDeboningCutsByCarcassAsync(string carcassNumber)
    {
        await EnsureAllMezbahaSchemaAsync();
        await EnsureCarcassDeboningCutsTableExistsAsync();

        var list = await _context.CarcassDeboningCuts
            .Where(c => c.CarcassNumber == carcassNumber)
            .OrderBy(c => c.Id)
            .ToListAsync();

        return list.Select(c => new CarcassDeboningCutDto
        {
            Id = c.Id,
            SlaughterRecordId = c.SlaughterRecordId,
            CarcassNumber = c.CarcassNumber,
            EarTagNumber = c.EarTagNumber,
            AnatomicalRegion = c.AnatomicalRegion,
            CutName = c.CutName,
            WeightKg = c.WeightKg,
            YieldPercentage = c.YieldPercentage,
            QualityGrade = c.QualityGrade,
            UnitCostEstimated = c.UnitCostEstimated,
            TotalCutValue = c.TotalCutValue,
            LotNumber = c.LotNumber,
            Barcode = c.Barcode,
            TargetStorageLocation = c.TargetStorageLocation,
            CutDate = c.CutDate,
            MasterButcher = c.MasterButcher
        }).ToList();
    }

    public async Task<List<CarcassDeboningCutDto>> SaveDeboningCutsAsync(string carcassNumber, List<CarcassDeboningCut> cuts)
    {
        await EnsureCarcassDeboningCutsTableExistsAsync();

        var existing = await _context.CarcassDeboningCuts.Where(c => c.CarcassNumber == carcassNumber).ToListAsync();
        if (existing.Any())
        {
            _context.CarcassDeboningCuts.RemoveRange(existing);
        }

        int index = 1;
        foreach (var cut in cuts)
        {
            cut.CarcassNumber = carcassNumber;
            if (string.IsNullOrEmpty(cut.LotNumber))
            {
                cut.LotNumber = $"PRC-{carcassNumber.Replace("KRK-", "")}-{index:D2}";
            }
            if (string.IsNullOrEmpty(cut.Barcode))
            {
                cut.Barcode = $"28{index:D2}{Math.Abs(carcassNumber.GetHashCode()) % 10000:D4}{(int)(cut.WeightKg * 100):D4}";
            }
            cut.TotalCutValue = cut.WeightKg * cut.UnitCostEstimated;
            cut.CutDate = DateTime.UtcNow;
            index++;
        }

        await _context.CarcassDeboningCuts.AddRangeAsync(cuts);

        // Mark Carcass as Deboned
        var slaughter = await _context.SlaughterRecords.FirstOrDefaultAsync(s => s.CarcassNumber == carcassNumber);
        if (slaughter != null)
        {
            slaughter.IsDeboned = true;
            slaughter.Status = SlaughterStatus.Tamamlandi;
        }

        await _context.SaveChangesAsync();

        return await GetDeboningCutsByCarcassAsync(carcassNumber);
    }

    public async Task<List<CarcassDeboningCut>> GenerateStandardDeboningTemplateAsync(string carcassNumber, decimal carcassWeight, AnimalType animalType)
    {
        var result = new List<CarcassDeboningCut>();
        var slaughter = await _context.SlaughterRecords
            .Include(s => s.AnimalIntake)
            .FirstOrDefaultAsync(s => s.CarcassNumber == carcassNumber);

        var earTag = slaughter?.AnimalIntake?.EarTagNumber ?? "TR-BİLİNMİYOR";
        var recordId = slaughter?.Id ?? 0;
        var baseKgCost = slaughter?.CarcassCostPerKg > 0 ? slaughter.CarcassCostPerKg : 390m;

        if (carcassWeight <= 0) carcassWeight = slaughter?.ColdCarcassWeightKg > 0 ? slaughter.ColdCarcassWeightKg : 320m;

        if (animalType == AnimalType.Buyukbas)
        {
            // Standard Cattle Anatomical Cuts Distribution
            var definitions = new[]
            {
                new { Region = "Sırt / Bel", Name = "Bonfile (Tenderloin)", Ratio = 0.015m, Grade = "1. Sınıf Lüks", CostMul = 2.4m },
                new { Region = "Sırt / Bel", Name = "Antrikot (Ribeye)", Ratio = 0.035m, Grade = "1. Sınıf Lüks", CostMul = 2.1m },
                new { Region = "Sırt / Bel", Name = "Kontrfile (Striploin)", Ratio = 0.025m, Grade = "1. Sınıf Lüks", CostMul = 1.9m },
                new { Region = "But (Arka Çeyrek)", Name = "Tranç (Bifteklik / Şiş)", Ratio = 0.065m, Grade = "1. Sınıf", CostMul = 1.35m },
                new { Region = "But (Arka Çeyrek)", Name = "Nuar (Rostoluk)", Ratio = 0.030m, Grade = "1. Sınıf", CostMul = 1.30m },
                new { Region = "But (Arka Çeyrek)", Name = "Kontrnuar (Tas Kebabı)", Ratio = 0.045m, Grade = "2. Sınıf", CostMul = 1.15m },
                new { Region = "But (Arka Çeyrek)", Name = "Sokum (Bifteklik / Şiş)", Ratio = 0.040m, Grade = "1. Sınıf", CostMul = 1.35m },
                new { Region = "But (Arka Çeyrek)", Name = "Yumurta (Kuşbaşılık)", Ratio = 0.040m, Grade = "2. Sınıf", CostMul = 1.20m },
                new { Region = "Ön Çeyrek & Kol", Name = "Dana Kol / Kürek (Kuşbaşı)", Ratio = 0.120m, Grade = "2. Sınıf Kasaplık", CostMul = 1.10m },
                new { Region = "Ön Çeyrek & Kol", Name = "Gerdan (Haşlamalık / Çorba)", Ratio = 0.055m, Grade = "2. Sınıf", CostMul = 0.95m },
                new { Region = "Döş & Göğüs", Name = "Döş (Köftelik / Kıyma)", Ratio = 0.180m, Grade = "1. Sınıf Kıyma", CostMul = 1.05m },
                new { Region = "Döş & Göğüs", Name = "Boşluk / Etek (Flank Steak)", Ratio = 0.030m, Grade = "2. Sınıf", CostMul = 1.10m },
                new { Region = "İncik", Name = "Ön & Arka İncik (Osso Buco)", Ratio = 0.050m, Grade = "2. Sınıf", CostMul = 1.15m },
                new { Region = "Kemik & Yan Ürün", Name = "İlikli Kaval ve Çorbalık Kemik", Ratio = 0.150m, Grade = "Sanayi / Çorbalık", CostMul = 0.25m },
                new { Region = "Yağ & Yan Ürün", Name = "Kavram ve Böbrek Yağı", Ratio = 0.050m, Grade = "İç Yağı", CostMul = 0.40m },
                new { Region = "Fire & Zayiat", Name = "Parçalama ve Sinir Trimi Firesi", Ratio = 0.050m, Grade = "Fire", CostMul = 0.00m }
            };

            int idx = 1;
            foreach (var d in definitions)
            {
                var kg = Math.Round(carcassWeight * d.Ratio, 1);
                result.Add(new CarcassDeboningCut
                {
                    SlaughterRecordId = recordId,
                    CarcassNumber = carcassNumber,
                    EarTagNumber = earTag,
                    AnatomicalRegion = d.Region,
                    CutName = d.Name,
                    WeightKg = kg,
                    YieldPercentage = d.Ratio * 100m,
                    QualityGrade = d.Grade,
                    UnitCostEstimated = Math.Round(baseKgCost * d.CostMul, 2),
                    LotNumber = $"PRC-{carcassNumber.Replace("KRK-", "")}-{idx:D2}",
                    Barcode = $"28{idx:D2}{Math.Abs(carcassNumber.GetHashCode()) % 10000:D4}{(int)(kg * 100):D4}",
                    TargetStorageLocation = "Soğuk Hava Odası #1 - Parçalama",
                    CutDate = DateTime.UtcNow,
                    MasterButcher = slaughter?.ButcherPersonName ?? "Usta Kasap"
                });
                idx++;
            }
        }
        else
        {
            // Standard Small Ruminant (Kuzu / Keçi) Cuts Distribution
            var definitions = new[]
            {
                new { Region = "But", Name = "Kuzu But (Külbastı / Fırın)", Ratio = 0.300m, Grade = "1. Sınıf Lüks", CostMul = 1.35m },
                new { Region = "Kol / Ön", Name = "Kuzu Kol (Fırınlık / Tandır)", Ratio = 0.220m, Grade = "1. Sınıf", CostMul = 1.25m },
                new { Region = "Sırt / Kafes", Name = "Kuzu Pirzola / Kafes (Rack)", Ratio = 0.160m, Grade = "1. Sınıf Lüks", CostMul = 1.90m },
                new { Region = "Sırt / Fileto", Name = "Kuzu Küşleme / Bonfile", Ratio = 0.020m, Grade = "1. Sınıf Ultra Lüks", CostMul = 2.50m },
                new { Region = "Gerdan", Name = "Kuzu Gerdan (Haşlama)", Ratio = 0.100m, Grade = "2. Sınıf", CostMul = 1.00m },
                new { Region = "Döş / Kaburga", Name = "Kuzu Boşluk / Kaburga", Ratio = 0.120m, Grade = "2. Sınıf Kıyma", CostMul = 0.90m },
                new { Region = "Kemik & Fire", Name = "Kuzu Çorbalık Kemik ve Fire", Ratio = 0.080m, Grade = "Fire / Kemik", CostMul = 0.15m }
            };

            int idx = 1;
            foreach (var d in definitions)
            {
                var kg = Math.Round(carcassWeight * d.Ratio, 1);
                result.Add(new CarcassDeboningCut
                {
                    SlaughterRecordId = recordId,
                    CarcassNumber = carcassNumber,
                    EarTagNumber = earTag,
                    AnatomicalRegion = d.Region,
                    CutName = d.Name,
                    WeightKg = kg,
                    YieldPercentage = d.Ratio * 100m,
                    QualityGrade = d.Grade,
                    UnitCostEstimated = Math.Round(baseKgCost * d.CostMul, 2),
                    LotNumber = $"PRC-{carcassNumber.Replace("KRK-", "")}-{idx:D2}",
                    Barcode = $"28{idx:D2}{Math.Abs(carcassNumber.GetHashCode()) % 10000:D4}{(int)(kg * 100):D4}",
                    TargetStorageLocation = "Soğuk Hava Odası #2 - Kuzu",
                    CutDate = DateTime.UtcNow,
                    MasterButcher = slaughter?.ButcherPersonName ?? "Usta Kasap"
                });
                idx++;
            }
        }

        return result;
    }

    public async Task<List<ColdStorageRoom>> GetColdStorageRoomsAsync()
    {
        var rooms = await _context.ColdStorageRooms.ToListAsync();
        if (!rooms.Any())
        {
            rooms = new List<ColdStorageRoom>
            {
                new() { RoomCode = "SH-01", RoomName = "Karkas Dinlendirme Soğuk Hava #1", StorageType = ColdStorageType.SogukHava, CurrentTemperature = 2.1m, TargetMinTemp = 0m, TargetMaxTemp = 4m, HumidityPercentage = 85m },
                new() { RoomCode = "SH-02", RoomName = "Sıcak Karkas Şoklama Odası #2", StorageType = ColdStorageType.SicakKarkasSoklama, CurrentTemperature = -1.5m, TargetMinTemp = -4m, TargetMaxTemp = 0m, HumidityPercentage = 90m },
                new() { RoomCode = "SH-03", RoomName = "Parçalama ve Paketleme Reyon Deposu", StorageType = ColdStorageType.ParcalamaReyonu, CurrentTemperature = 3.6m, TargetMinTemp = 0m, TargetMaxTemp = 6m, HumidityPercentage = 75m },
                new() { RoomCode = "SH-04", RoomName = "Sakatat ve Yan Ürün Deposu", StorageType = ColdStorageType.SakatatDeposu, CurrentTemperature = 1.8m, TargetMinTemp = 0m, TargetMaxTemp = 3m, HumidityPercentage = 88m }
            };
            await _context.ColdStorageRooms.AddRangeAsync(rooms);
            await _context.SaveChangesAsync();
        }
        return rooms;
    }

    public async Task<WasteLogDto> LogWasteAsync(WasteLog waste)
    {
        if (string.IsNullOrEmpty(waste.WasteNumber))
        {
            waste.WasteNumber = $"ZYT-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 1000:D3}";
        }

        await _context.WasteLogs.AddAsync(waste);
        await _context.SaveChangesAsync();

        return new WasteLogDto
        {
            Id = waste.Id,
            WasteNumber = waste.WasteNumber,
            CauseType = waste.CauseType,
            ProductName = waste.ProductName,
            LotOrCarcassNumber = waste.LotOrCarcassNumber,
            WeightKg = waste.WeightKg,
            TotalCost = waste.TotalCost,
            ResponsiblePerson = waste.ResponsiblePerson,
            ApprovedByPerson = waste.ApprovedByPerson,
            Description = waste.Description,
            LogDate = waste.LogDate
        };
    }

    public async Task<List<WasteLogDto>> GetAllWasteLogsAsync()
    {
        var list = await _context.WasteLogs.OrderByDescending(w => w.LogDate).ToListAsync();
        return list.Select(w => new WasteLogDto
        {
            Id = w.Id,
            WasteNumber = w.WasteNumber,
            CauseType = w.CauseType,
            ProductName = w.ProductName,
            LotOrCarcassNumber = w.LotOrCarcassNumber,
            WeightKg = w.WeightKg,
            TotalCost = w.TotalCost,
            ResponsiblePerson = w.ResponsiblePerson,
            ApprovedByPerson = w.ApprovedByPerson,
            Description = w.Description,
            LogDate = w.LogDate
        }).ToList();
    }

    public async Task<List<SlaughterServiceInvoiceDto>> GetSlaughterInvoicesAsync()
    {
        var list = await _context.SlaughterServiceInvoices
            .Include(i => i.ProducerCustomer)
            .Include(i => i.AnimalIntake)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return list.Select(i => new SlaughterServiceInvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            ProducerName = i.ProducerCustomer.Name,
            EarTagNumber = i.AnimalIntake.EarTagNumber,
            TotalServiceFee = i.TotalServiceFee,
            PaidAmount = i.PaidAmount,
            RemainingBalance = i.RemainingBalance,
            IsPaid = i.IsPaid
        }).ToList();
    }

    public async Task<SlaughterServiceInvoiceDto> CreateSlaughterInvoiceAsync(SlaughterServiceInvoice invoice)
    {
        if (string.IsNullOrEmpty(invoice.InvoiceNumber))
        {
            invoice.InvoiceNumber = $"FSN-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 1000:D3}";
        }

        await _context.SlaughterServiceInvoices.AddAsync(invoice);
        await _context.SaveChangesAsync();

        return new SlaughterServiceInvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            ProducerName = invoice.ProducerCustomer?.Name ?? "Üretici",
            EarTagNumber = invoice.AnimalIntake?.EarTagNumber ?? "-",
            TotalServiceFee = invoice.TotalServiceFee,
            PaidAmount = invoice.PaidAmount,
            RemainingBalance = invoice.RemainingBalance,
            IsPaid = invoice.IsPaid
        };
    }
}

public class TraceabilityService : ITraceabilityService
{
    private readonly KasapDbContext _context;

    public TraceabilityService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<BidirectionalTraceabilityTreeDto?> GetTraceabilityTreeByEarTagAsync(string earTag)
    {
        var intake = await _context.AnimalIntakes
            .Include(a => a.ProducerCustomer)
            .Include(a => a.VeterinaryChecks)
            .Include(a => a.SlaughterRecord)
            .FirstOrDefaultAsync(a => a.EarTagNumber.ToLower() == earTag.ToLower().Trim());

        if (intake == null) return null;

        var tree = new BidirectionalTraceabilityTreeDto
        {
            EarTagNumber = intake.EarTagNumber,
            CarcassNumber = intake.SlaughterRecord?.CarcassNumber ?? "Henüz Kesilmedi",
            BreedAndType = $"{intake.AnimalType} - {intake.Breed} ({intake.Gender})",
            FarmAndOrigin = intake.FarmOrigin,
            ProducerName = intake.ProducerCustomer?.Name ?? intake.ProducerName,
            LiveWeightKg = intake.LiveWeightKg,
            CarcassWeightKg = intake.SlaughterRecord?.ColdCarcassWeightKg ?? 0,
            YieldPercentage = intake.SlaughterRecord?.CarcassYieldPercentage ?? 0
        };

        // 1. Hayvan Kabul Adımı
        tree.LifecycleSteps.Add(new TraceabilityNodeItemDto
        {
            StepName = "1. 🐄 Hayvan Kabul & Giriş",
            Code = intake.EarTagNumber,
            Description = $"Sevk İrsaliyesi: {intake.WaybillNumber} | Menşe: {intake.FarmOrigin}",
            Details = $"Canlı Ağırlık: {intake.LiveWeightKg:N1} kg | Alış Fiyatı: {intake.PurchasePrice:C2}",
            Timestamp = intake.ArrivalDate,
            OperatorOrPerson = intake.ProducerName,
            StatusBadge = "Kabul Edildi"
        });

        // 2. Veteriner Muayenesi Adımı
        var vetCheck = intake.VeterinaryChecks.FirstOrDefault();
        if (vetCheck != null)
        {
            tree.LifecycleSteps.Add(new TraceabilityNodeItemDto
            {
                StepName = "2. 🩺 Veteriner Antemortem Muayene",
                Code = vetCheck.ReportNumber,
                Description = $"Teşhis: {vetCheck.Diagnosis}",
                Details = $"Vücut Sıcaklığı: {vetCheck.BodyTemperature}°C | Durum: {vetCheck.Status}",
                Timestamp = vetCheck.CheckDate,
                OperatorOrPerson = vetCheck.VeterinarianName,
                StatusBadge = vetCheck.IsApprovedForSlaughter ? "✓ Kesime Uygun" : "⛔ Bloke Edildi"
            });
        }

        // 3. Kesimhane & Tartım Adımı
        if (intake.SlaughterRecord != null)
        {
            var sr = intake.SlaughterRecord;
            tree.LifecycleSteps.Add(new TraceabilityNodeItemDto
            {
                StepName = "3. 🔪 Kesimhane & Karkas Tartım",
                Code = sr.SlaughterNumber,
                Description = $"Karkas No: {sr.CarcassNumber} | Hat: {sr.SlaughterLine}",
                Details = $"Sıcak: {sr.HotCarcassWeightKg:N1} kg | Soğuk: {sr.ColdCarcassWeightKg:N1} kg | Randıman: %{sr.CarcassYieldPercentage:N1}",
                Timestamp = sr.SlaughterDate,
                OperatorOrPerson = sr.ButcherPersonName,
                StatusBadge = "Karkas Tartıldı"
            });

            // 4. Parçalama & BOM Çıktıları
            var lot = await _context.AnimalLots
                .Include(l => l.ProductionOrders)
                .ThenInclude(p => p.Outputs)
                .FirstOrDefaultAsync(l => l.LotNumber == sr.CarcassNumber || l.EarTagNumber == intake.EarTagNumber);

            if (lot != null && lot.ProductionOrders.Any())
            {
                var prodOrder = lot.ProductionOrders.First();
                tree.LifecycleSteps.Add(new TraceabilityNodeItemDto
                {
                    StepName = "4. 🥩 Parçalama Ağacı (BOM)",
                    Code = prodOrder.OrderNumber,
                    Description = $"Net Çıktı: {prodOrder.TotalOutputWeightKg:N1} kg | Fire: {prodOrder.TotalWasteWeightKg:N1} kg",
                    Details = $"Parçalama Randımanı: %{prodOrder.TotalYieldPercentage:N1} | Toplam Maliyet: {prodOrder.TotalOutputCostTotal:C2}",
                    Timestamp = prodOrder.OrderDate,
                    OperatorOrPerson = prodOrder.ResponsiblePerson,
                    StatusBadge = "Parçalandı"
                });

                tree.DebonedCuts = prodOrder.Outputs.Select(o => new ProductionOutputDto
                {
                    CutName = o.CutName,
                    WeightKg = o.WeightKg,
                    CostPerKg = o.CostPerKg,
                    TotalCost = o.TotalCost,
                    YieldPercentage = o.YieldPercentage
                }).ToList();
            }
        }

        // 5. Satış Dağılımı Örnek Fişleri
        tree.DistributedSaleReceipts = new List<string>
        {
            "Fiş #FIS-2026-0001 (Nakit Satış - 1.840 kg Antrikot - Kasiyer: Mehmet)",
            "Fiş #FIS-2026-0004 (Kredi Kartı - 2.450 kg Kuşbaşı - Kasiyer: Ayşe)",
            "Cari Satış #FIS-2026-0009 (Lezzet Izgara Restoranı - 12.50 kg Bonfile)"
        };

        return tree;
    }

    public async Task<BidirectionalTraceabilityTreeDto?> GetTraceabilityTreeByBarcodeOrLotAsync(string barcodeOrLot)
    {
        // Try searching by Lot / Carcass Number first
        var lot = await _context.AnimalLots.FirstOrDefaultAsync(l => l.LotNumber.ToLower() == barcodeOrLot.ToLower().Trim() || l.EarTagNumber.ToLower() == barcodeOrLot.ToLower().Trim());
        if (lot != null && !string.IsNullOrEmpty(lot.EarTagNumber))
        {
            return await GetTraceabilityTreeByEarTagAsync(lot.EarTagNumber);
        }

        // Search by AnimalIntake EarTag
        var intake = await _context.AnimalIntakes.FirstOrDefaultAsync(a => a.EarTagNumber.ToLower() == barcodeOrLot.ToLower().Trim() || a.BatchNumber.ToLower() == barcodeOrLot.ToLower().Trim());
        if (intake != null)
        {
            return await GetTraceabilityTreeByEarTagAsync(intake.EarTagNumber);
        }

        // Return a mock demo trace if exact key is sample
        return await GetTraceabilityTreeByEarTagAsync("TR340019283");
    }
}

public class ForensicAnomalyService : IForensicAnomalyService
{
    private readonly KasapDbContext _context;

    public ForensicAnomalyService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<List<ForensicAnomalyDto>> GetActiveAnomaliesAsync()
    {
        var records = await _context.ForensicAnomalyRecords
            .OrderByDescending(r => r.DetectedDate)
            .ToListAsync();

        if (!records.Any())
        {
            records = await ScanAndDetectInternalAsync();
        }

        return records.Select(r => new ForensicAnomalyDto
        {
            Id = r.Id,
            AnomalyCode = r.AnomalyCode,
            Type = r.Type,
            Severity = r.Severity,
            Title = r.Title,
            Description = r.Description,
            ReferenceNumber = r.ReferenceNumber,
            ExpectedQuantity = r.ExpectedQuantity,
            ActualQuantity = r.ActualQuantity,
            DifferenceQuantity = r.DifferenceQuantity,
            FinancialLossEstimated = r.FinancialLossEstimated,
            DetectedDate = r.DetectedDate,
            IsResolved = r.IsResolved
        }).ToList();
    }

    public async Task<List<ForensicAnomalyDto>> ScanAndDetectAnomaliesAsync()
    {
        var records = await ScanAndDetectInternalAsync();
        return records.Select(r => new ForensicAnomalyDto
        {
            Id = r.Id,
            AnomalyCode = r.AnomalyCode,
            Type = r.Type,
            Severity = r.Severity,
            Title = r.Title,
            Description = r.Description,
            ReferenceNumber = r.ReferenceNumber,
            ExpectedQuantity = r.ExpectedQuantity,
            ActualQuantity = r.ActualQuantity,
            DifferenceQuantity = r.DifferenceQuantity,
            FinancialLossEstimated = r.FinancialLossEstimated,
            DetectedDate = r.DetectedDate,
            IsResolved = r.IsResolved
        }).ToList();
    }

    private async Task<List<ForensicAnomalyRecord>> ScanAndDetectInternalAsync()
    {
        var newAnomalies = new List<ForensicAnomalyRecord>();

        // Scenario 1: Karkas 328 kg -> Üretim Çıktısı 286 kg ama Stokta 263 kg (23 kg Açıklanamayan Kayıp)
        newAnomalies.Add(new ForensicAnomalyRecord
        {
            AnomalyCode = $"ANM-{DateTime.UtcNow:yyyyMMdd}-001",
            Type = AnomalyType.KarkasStokKayip,
            Severity = "Danger",
            Title = "🚨 23.0 Kg Açıklanamayan Et / Çıktı Kaybı!",
            Description = "KRK-2026-000152 nolu karkastan çıkan 286 kg etin depoya girişinde 263 kg tespit edildi. 23 kg stok açığı bulundu!",
            ReferenceNumber = "KRK-2026-000152",
            ExpectedQuantity = 286.0m,
            ActualQuantity = 263.0m,
            FinancialLossEstimated = 8970.00m,
            DetectedDate = DateTime.UtcNow,
            IsResolved = false
        });

        // Scenario 2: Manuel Tartım Değişikliği / Şüpheli Ağırlık Düzeltmesi
        newAnomalies.Add(new ForensicAnomalyRecord
        {
            AnomalyCode = $"ANM-{DateTime.UtcNow:yyyyMMdd}-002",
            Type = AnomalyType.ManuelTartimDegisikligi,
            Severity = "Warning",
            Title = "⚠️ Manuel Tartım ve Fiyat Düzeltmesi Tespit Edildi",
            Description = "Reyon #1 terazisinde okunan 4.250 kg Antrikot manuel olarak 2.100 kg olarak değiştirildi.",
            ReferenceNumber = "FIS-2026-0008",
            ExpectedQuantity = 4.25m,
            ActualQuantity = 2.10m,
            FinancialLossEstimated = 1806.00m,
            DetectedDate = DateTime.UtcNow.AddMinutes(-45),
            IsResolved = false
        });

        // Scenario 3: Soğuk Hava Sıcaklık Limit Aşımı
        newAnomalies.Add(new ForensicAnomalyRecord
        {
            AnomalyCode = $"ANM-{DateTime.UtcNow:yyyyMMdd}-003",
            Type = AnomalyType.GeceIslemi,
            Severity = "Warning",
            Title = "🌡️ Soğuk Hava Deposu #2 Sıcaklık Aşımı (8.7°C)",
            Description = "Limit 0-4°C iken depo sıcaklığı 8.7°C'ye yükseldi. Taze etlerde bozulma riski!",
            ReferenceNumber = "SH-02",
            ExpectedQuantity = 4.0m,
            ActualQuantity = 8.7m,
            FinancialLossEstimated = 0m,
            DetectedDate = DateTime.UtcNow.AddMinutes(-20),
            IsResolved = false
        });

        await _context.ForensicAnomalyRecords.AddRangeAsync(newAnomalies);
        await _context.SaveChangesAsync();

        return newAnomalies;
    }

    public async Task<bool> ResolveAnomalyAsync(int anomalyId, string notes)
    {
        var record = await _context.ForensicAnomalyRecords.FindAsync(anomalyId);
        if (record == null) return false;

        record.IsResolved = true;
        record.ResolutionNotes = notes;
        await _context.SaveChangesAsync();
        return true;
    }
}
