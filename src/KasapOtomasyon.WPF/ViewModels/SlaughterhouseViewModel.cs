using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.WPF.Services;

namespace KasapOtomasyon.WPF.ViewModels;

public partial class SlaughterhouseViewModel : ObservableObject
{
    // SEUROP & Quality Grading Properties
    [ObservableProperty] private string conformationClass = "R"; // S, E, U, R, O, P
    [ObservableProperty] private int fatCoverScore = 3;          // 1 - 5
    [ObservableProperty] private int marblingScore = 5;          // 1 - 9 BMS
    [ObservableProperty] private decimal postMortemPh24 = 5.65m; // Normal: 5.4 - 5.8
    [ObservableProperty] private string hookRailNumber = "Ray-1 / Askı-14";

    // Collections
    public ObservableCollection<AnimalIntakeDto> AnimalIntakes { get; } = new();
    public ObservableCollection<AnimalIntakeDto> FilteredAnimalIntakes { get; } = new();
    public ObservableCollection<SlaughterRecordDto> SlaughterRecords { get; } = new();
    public ObservableCollection<ColdStorageRoom> ColdStorageRooms { get; } = new();
    public ObservableCollection<WasteLogDto> WasteLogs { get; } = new();
    public ObservableCollection<ForensicAnomalyDto> Anomalies { get; } = new();
    public ObservableCollection<SlaughterServiceInvoiceDto> Invoices { get; } = new();
    
    // Deboning Anatomical Cuts Collection
    public ObservableCollection<CarcassDeboningCut> DeboningCuts { get; } = new();
    
    // Dynamic Breeds & Genders
    public ObservableCollection<string> AvailableBreeds { get; } = new();
    public ObservableCollection<string> AvailableGenders { get; } = new()
    {
        "Tosun", "Düve", "Dana", "Boğa", "İnek", "Koç", "Koyun", "Kuzu", "Erkeç", "Keçi", "Oğlak"
    };

    public ObservableCollection<string> ConformationClasses { get; } = new()
    {
        "S (Üstün - Superior)",
        "E (Mükemmel - Excellent)",
        "U (Çok İyi - Very Good)",
        "R (İyi - Good)",
        "O (Orta - Fair)",
        "P (Zayıf - Poor)"
    };

    // Anatomical Cut Helpers
    public ObservableCollection<string> AnatomicalRegionsList { get; } = new()
    {
        "Sırt / Bel (Asil Etler)",
        "But (Arka Çeyrek)",
        "Ön Çeyrek & Kol / Kürek",
        "Gerdan",
        "Döş & Göğüs / Kaburga",
        "İncik (Ön/Arka)",
        "Kemik / İlikli Kaval",
        "Yağ (Kavram/Böbrek)",
        "Sakatat / Yan Ürün",
        "Fire / Trim"
    };

    public ObservableCollection<string> PopularCutsList { get; } = new()
    {
        "Antrikot (Ribeye)",
        "Bonfile (Tenderloin)",
        "Kontrfile (Striploin)",
        "Tranç (Bifteklik / Şiş)",
        "Nuar (Rostoluk)",
        "Kontrnuar (Tas Kebabı)",
        "Sokum (Bifteklik)",
        "Yumurta (Kuşbaşılık)",
        "Dana Kol / Kürek (Kuşbaşı)",
        "Gerdan (Haşlamalık / Çorba)",
        "Döş (Köftelik / Kıyma)",
        "Boşluk / Etek (Flank Steak)",
        "Ön & Arka İncik (Osso Buco)",
        "İlikli Kaval ve Çorbalık Kemik",
        "Kavram ve Böbrek Yağı",
        "Kuzu But (Külbastı / Fırın)",
        "Kuzu Kol (Fırınlık / Tandır)",
        "Kuzu Pirzola / Kafes",
        "Kuzu Küşleme / Bonfile",
        "Kuzu Gerdan",
        "Kuzu Boşluk / Kaburga"
    };

    // ==========================================
    // SELECTION & SEARCH FILTERS
    // ==========================================
    [ObservableProperty] private AnimalIntakeDto? selectedAnimal;
    [ObservableProperty] private SlaughterRecordDto? selectedSlaughterRecord;
    [ObservableProperty] private string searchFilterText = string.Empty;
    [ObservableProperty] private string selectedStatusFilter = "ALL"; // ALL, APPROVED, BLOCKED, SLAUGHTERED

    // ==========================================
    // POPUP VISIBILITIES
    // ==========================================
    [ObservableProperty] private bool isAddAnimalModalOpen;
    [ObservableProperty] private bool isVetModalOpen;
    [ObservableProperty] private bool isSlaughterModalOpen;
    [ObservableProperty] private bool isDeboningDialogOpen;
    [ObservableProperty] private bool isTraceabilityModalOpen;
    [ObservableProperty] private bool isPassportDialogOpen;
    [ObservableProperty] private bool isWasteModalOpen;
    [ObservableProperty] private bool isInvoiceModalOpen;
    [ObservableProperty] private bool isAddBreedPopupOpen;
    [ObservableProperty] private bool isTrueCostDialogOpen;
    [ObservableProperty] private bool isRecallDialogOpen;

    // ==========================================
    // ANIMAL PASSPORT DOCUMENT
    // ==========================================
    [ObservableProperty] private AnimalPassportDocumentDto? currentPassportDocument;

    // ==========================================
    // FORM 1: HAYVAN KABUL & GİRİŞ
    // ==========================================
    [ObservableProperty] private string newEarTagNumber = string.Empty;
    [ObservableProperty] private string newPassportNumber = string.Empty;
    [ObservableProperty] private AnimalType selectedAnimalType = AnimalType.Buyukbas;
    [ObservableProperty] private string selectedBreed = "Simental (Simmental)";
    [ObservableProperty] private string selectedGenderString = "Tosun";
    [ObservableProperty] private int newAgeMonths = 0;
    [ObservableProperty] private decimal newLiveWeightKg = 0m;
    [ObservableProperty] private decimal newPurchasePrice = 0m;
    [ObservableProperty] private string newProducerName = string.Empty;
    [ObservableProperty] private string newWaybillNumber = string.Empty;
    [ObservableProperty] private string newFarmOrigin = string.Empty;
    [ObservableProperty] private string newRfidTag = string.Empty;
    [ObservableProperty] private string newNotes = string.Empty;
    
    // Custom Breed Quick Add
    [ObservableProperty] private string customNewBreedName = string.Empty;

    // ==========================================
    // FORM 2: VETERİNER MUAYENE & KESİM ONAYI
    // ==========================================
    [ObservableProperty] private string vetDoctorName = string.Empty;
    [ObservableProperty] private decimal vetBodyTemperature = 38.6m;
    [ObservableProperty] private VeterinaryCheckStatus selectedVetStatus = VeterinaryCheckStatus.Uygun;
    [ObservableProperty] private bool isApprovedForSlaughter = true;
    [ObservableProperty] private string vetDiagnosis = "Klinik muayenede patolojik bulguya rastlanmadı. Kesime uygundur.";
    [ObservableProperty] private string vetReportNumber = string.Empty;

    // ==========================================
    // FORM 3: KESİMHANE & KARKAS TARTIM & RANDIMAN
    // ==========================================
    [ObservableProperty] private string butcherName = string.Empty;
    [ObservableProperty] private string slaughterLine = "Hat-A (Büyükbaş)";
    [ObservableProperty] private decimal slaughterLiveWeightKg = 0m;
    [ObservableProperty] private decimal hotCarcassWeightKg = 0m;
    [ObservableProperty] private decimal coldCarcassWeightKg = 0m;
    [ObservableProperty] private decimal headWeightKg = 0m;
    [ObservableProperty] private decimal hideWeightKg = 0m;
    [ObservableProperty] private decimal offalWeightKg = 0m;
    [ObservableProperty] private decimal fatWeightKg = 0m;
    [ObservableProperty] private decimal boneWeightKg = 0m;
    [ObservableProperty] private decimal slaughterWasteKg = 0m;
    
    // Costing
    [ObservableProperty] private decimal animalPurchaseCost = 0m;
    [ObservableProperty] private decimal slaughterLaborCost = 2500m;
    [ObservableProperty] private decimal transportationCost = 0m;
    [ObservableProperty] private decimal coolingCost = 1500m;
    [ObservableProperty] private decimal overheadCost = 2000m;

    public decimal CalculatedYieldPercentage => SlaughterLiveWeightKg > 0 ? Math.Round((HotCarcassWeightKg / SlaughterLiveWeightKg) * 100m, 2) : 0;
    public decimal CalculatedTotalCost => AnimalPurchaseCost + SlaughterLaborCost + TransportationCost + CoolingCost + OverheadCost;
    public decimal CalculatedCostPerKg => ColdCarcassWeightKg > 0 ? Math.Round(CalculatedTotalCost / ColdCarcassWeightKg, 2) : 0;

    // ==========================================
    // FORM 4: KARKAS PARÇALAMA & ANATOMİK ET DÖKÜMÜ
    // ==========================================
    [ObservableProperty] private string deboningCarcassNumber = string.Empty;
    [ObservableProperty] private string deboningEarTagNumber = string.Empty;
    [ObservableProperty] private decimal deboningTotalCarcassKg = 0m;
    [ObservableProperty] private decimal deboningAssignedTotalKg = 0m;
    [ObservableProperty] private decimal deboningRemainingKg = 0m;
    [ObservableProperty] private decimal deboningEfficiencyYield = 0m;
    [ObservableProperty] private decimal deboningTotalCutValue = 0m;

    // In-place Add Single Cut Inputs
    [ObservableProperty] private string newCutAnatomicalRegion = "Sırt / Bel (Asil Etler)";
    [ObservableProperty] private string newCutName = "Antrikot (Ribeye)";
    [ObservableProperty] private decimal newCutWeightKg = 0m;
    [ObservableProperty] private string newCutQualityGrade = "1. Sınıf Lüks";
    [ObservableProperty] private decimal newCutUnitCost = 0m;
    [ObservableProperty] private string newCutStorageLocation = "Soğuk Hava Odası #1 - Parçalama";

    // ==========================================
    // FORM 5: İZLENEBİLİRLİK (TRACEABILITY)
    // ==========================================
    [ObservableProperty] private string searchTraceabilityQuery = string.Empty;
    [ObservableProperty] private BidirectionalTraceabilityTreeDto? currentTraceTree;

    // ==========================================
    // FORM 6: FİRE GİRİŞİ FORMU
    // ==========================================
    [ObservableProperty] private WasteCauseType newWasteCause = WasteCauseType.ParcalamaFiresi;
    [ObservableProperty] private string newWasteProductName = string.Empty;
    [ObservableProperty] private string newWasteLot = string.Empty;
    [ObservableProperty] private decimal newWasteWeightKg = 0m;
    [ObservableProperty] private decimal newWasteCostPerKg = 0m;
    [ObservableProperty] private string newWasteReason = string.Empty;

    private readonly ISlaughterhouseService _slaughterService;
    private readonly ITraceabilityService _traceabilityService;
    private readonly IForensicAnomalyService _anomalyService;
    private readonly IScaleIntegrationService _scaleService;
    private readonly IGs1LabelService _labelService;
    private readonly IAnimalPassportPdfService _passportPdfService;
    private readonly IMassBalanceAndCuttingService _massBalanceService;
    private readonly IFoodRecallAndHaccpService _recallService;
    private readonly ITrueCostEngineService _trueCostService;
    private readonly IAntiFraudEngineService _fraudService;
    private readonly INavigationService _navService;

    // Enterprise Results
    [ObservableProperty] private MassBalanceCalculationResult? lastMassBalanceResult;
    [ObservableProperty] private FoodRecallAnalysisResult? lastRecallResult;
    [ObservableProperty] private TrueCostRollup? currentTrueCostRollup;
    [ObservableProperty] private FraudRiskEvaluationResult? lastFraudRiskResult;

    // General Status Message
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string statusColor = "#16A34A";

    public SlaughterhouseViewModel(
        ISlaughterhouseService slaughterService,
        ITraceabilityService traceabilityService,
        IForensicAnomalyService anomalyService,
        IScaleIntegrationService scaleService,
        IGs1LabelService labelService,
        IAnimalPassportPdfService passportPdfService,
        IMassBalanceAndCuttingService massBalanceService,
        IFoodRecallAndHaccpService recallService,
        ITrueCostEngineService trueCostService,
        IAntiFraudEngineService fraudService,
        INavigationService navService)
    {
        _slaughterService = slaughterService;
        _traceabilityService = traceabilityService;
        _anomalyService = anomalyService;
        _scaleService = scaleService;
        _labelService = labelService;
        _passportPdfService = passportPdfService;
        _massBalanceService = massBalanceService;
        _recallService = recallService;
        _trueCostService = trueCostService;
        _fraudService = fraudService;
        _navService = navService;

        _ = LoadAllDataAsync();
    }

    [RelayCommand]
    public async Task LoadAllDataAsync()
    {
        try
        {
            // Load Breeds
            var breeds = await _slaughterService.GetAllBreedsAsync();
            AvailableBreeds.Clear();
            foreach (var b in breeds) AvailableBreeds.Add(b);
            if (!string.IsNullOrEmpty(SelectedBreed) && !AvailableBreeds.Contains(SelectedBreed))
            {
                AvailableBreeds.Insert(0, SelectedBreed);
            }

            var intakes = await _slaughterService.GetAllIntakesAsync();
            AnimalIntakes.Clear();
            foreach (var item in intakes) AnimalIntakes.Add(item);

            ApplyFilter();

            var slaughters = await _slaughterService.GetAllSlaughterRecordsAsync();
            SlaughterRecords.Clear();
            foreach (var item in slaughters) SlaughterRecords.Add(item);

            var rooms = await _slaughterService.GetColdStorageRoomsAsync();
            ColdStorageRooms.Clear();
            foreach (var item in rooms) ColdStorageRooms.Add(item);

            var wastes = await _slaughterService.GetAllWasteLogsAsync();
            WasteLogs.Clear();
            foreach (var item in wastes) WasteLogs.Add(item);

            var anomalies = await _anomalyService.GetActiveAnomaliesAsync();
            Anomalies.Clear();
            foreach (var item in anomalies) Anomalies.Add(item);

            var invoices = await _slaughterService.GetSlaughterInvoicesAsync();
            Invoices.Clear();
            foreach (var item in invoices) Invoices.Add(item);

            if (SelectedAnimal == null && AnimalIntakes.Any())
            {
                SelectedAnimal = AnimalIntakes.First();
            }

            if (SelectedAnimal != null)
            {
                SearchTraceabilityQuery = SelectedAnimal.EarTagNumber;
                CurrentTraceTree = await _traceabilityService.GetTraceabilityTreeByEarTagAsync(SelectedAnimal.EarTagNumber);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Veriler yüklenirken hata: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task AddCustomBreedAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomNewBreedName))
        {
            ShowStatus("Lütfen eklenecek ırk adını yazın.", "#DC2626");
            return;
        }

        try
        {
            var breed = await _slaughterService.AddBreedAsync(CustomNewBreedName, SelectedAnimalType);
            if (!AvailableBreeds.Contains(breed.Name))
            {
                AvailableBreeds.Add(breed.Name);
            }
            SelectedBreed = breed.Name;
            CustomNewBreedName = string.Empty;
            IsAddBreedPopupOpen = false;
            ShowStatus($"✓ Yeni ırk '{breed.Name}' başarıyla eklendi.", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Irk ekleme hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public void ToggleAddBreedPopup()
    {
        IsAddBreedPopupOpen = !IsAddBreedPopupOpen;
    }

    // ==========================================
    // FILTER & SEARCH LOGIC
    // ==========================================
    [RelayCommand]
    public void SearchAnimals()
    {
        ApplyFilter();
    }

    [RelayCommand]
    public void FilterByStatus(string status)
    {
        SelectedStatusFilter = status;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = AnimalIntakes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchFilterText))
        {
            var text = SearchFilterText.Trim().ToLowerInvariant();
            query = query.Where(a =>
                a.EarTagNumber.ToLowerInvariant().Contains(text) ||
                a.ProducerName.ToLowerInvariant().Contains(text) ||
                a.Breed.ToLowerInvariant().Contains(text) ||
                a.WaybillNumber.ToLowerInvariant().Contains(text) ||
                a.FarmOrigin.ToLowerInvariant().Contains(text));
        }

        switch (SelectedStatusFilter)
        {
            case "APPROVED":
                query = query.Where(a => a.IsApprovedForSlaughter && a.SlaughterStatus != SlaughterStatus.Tamamlandi);
                break;
            case "BLOCKED":
                query = query.Where(a => !a.IsApprovedForSlaughter);
                break;
            case "SLAUGHTERED":
                query = query.Where(a => a.SlaughterStatus == SlaughterStatus.Tamamlandi);
                break;
        }

        FilteredAnimalIntakes.Clear();
        foreach (var item in query)
        {
            FilteredAnimalIntakes.Add(item);
        }
    }

    // ==========================================
    // POPUP OPEN / CLOSE HANDLERS
    // ==========================================
    [RelayCommand]
    public void OpenAddAnimalModal()
    {
        NewEarTagNumber = string.Empty;
        NewPassportNumber = string.Empty;
        NewWaybillNumber = string.Empty;
        NewRfidTag = string.Empty;
        NewProducerName = string.Empty;
        NewFarmOrigin = string.Empty;
        NewNotes = string.Empty;
        NewLiveWeightKg = 0m;
        NewPurchasePrice = 0m;
        NewAgeMonths = 0;
        
        IsAddAnimalModalOpen = true;
    }

    [RelayCommand]
    public void CloseAddAnimalModal() => IsAddAnimalModalOpen = false;

    [RelayCommand]
    public void OpenVetModal(AnimalIntakeDto? targetAnimal)
    {
        if (targetAnimal != null) SelectedAnimal = targetAnimal;
        if (SelectedAnimal == null)
        {
            ShowStatus("Lütfen muayene edilecek bir hayvan seçin.", "#DC2626");
            return;
        }
        VetReportNumber = $"VET-{DateTime.UtcNow:yyyyMMdd}-{SelectedAnimal.Id:D4}";
        IsVetModalOpen = true;
    }

    [RelayCommand]
    public void CloseVetModal() => IsVetModalOpen = false;

    [RelayCommand]
    public void OpenSlaughterModal(AnimalIntakeDto? targetAnimal)
    {
        if (targetAnimal != null) SelectedAnimal = targetAnimal;
        if (SelectedAnimal == null)
        {
            ShowStatus("Lütfen kesim yapılacak hayvanı seçin.", "#DC2626");
            return;
        }

        if (!SelectedAnimal.IsApprovedForSlaughter)
        {
            ShowStatus($"HAYVAN KESİLEMEZ: {SelectedAnimal.EarTagNumber} Veteriner Tarafından Bloke Edilmiştir!", "#DC2626");
            return;
        }

        SlaughterLiveWeightKg = SelectedAnimal.LiveWeightKg;
        AnimalPurchaseCost = SelectedAnimal.PurchasePrice;
        HotCarcassWeightKg = SelectedAnimal.LiveWeightKg > 0 ? Math.Round(SelectedAnimal.LiveWeightKg * 0.54m, 1) : 0;
        ColdCarcassWeightKg = Math.Round(HotCarcassWeightKg * 0.98m, 1);

        IsSlaughterModalOpen = true;
    }

    [RelayCommand]
    public void CloseSlaughterModal() => IsSlaughterModalOpen = false;

    // ==========================================
    // DEBONING & ANATOMICAL CUTS WORKSPACE
    // ==========================================
    [RelayCommand]
    public async Task OpenDeboningDialogAsync(SlaughterRecordDto? record)
    {
        if (record != null)
        {
            SelectedSlaughterRecord = record;
            DeboningCarcassNumber = record.CarcassNumber;
            DeboningEarTagNumber = record.EarTagNumber;
            DeboningTotalCarcassKg = record.ColdCarcassWeightKg > 0 ? record.ColdCarcassWeightKg : record.HotCarcassWeightKg;
        }
        else if (SlaughterRecords.Any())
        {
            var first = SlaughterRecords.First();
            SelectedSlaughterRecord = first;
            DeboningCarcassNumber = first.CarcassNumber;
            DeboningEarTagNumber = first.EarTagNumber;
            DeboningTotalCarcassKg = first.ColdCarcassWeightKg > 0 ? first.ColdCarcassWeightKg : first.HotCarcassWeightKg;
        }

        // Load existing deboning cuts from DB
        var existing = await _slaughterService.GetDeboningCutsByCarcassAsync(DeboningCarcassNumber);
        DeboningCuts.Clear();
        if (existing.Any())
        {
            foreach (var item in existing)
            {
                DeboningCuts.Add(new CarcassDeboningCut
                {
                    Id = item.Id,
                    SlaughterRecordId = item.SlaughterRecordId,
                    CarcassNumber = item.CarcassNumber,
                    EarTagNumber = item.EarTagNumber,
                    AnatomicalRegion = item.AnatomicalRegion,
                    CutName = item.CutName,
                    WeightKg = item.WeightKg,
                    YieldPercentage = item.YieldPercentage,
                    QualityGrade = item.QualityGrade,
                    UnitCostEstimated = item.UnitCostEstimated,
                    TotalCutValue = item.TotalCutValue,
                    LotNumber = item.LotNumber,
                    Barcode = item.Barcode,
                    TargetStorageLocation = item.TargetStorageLocation,
                    CutDate = item.CutDate,
                    MasterButcher = item.MasterButcher
                });
            }
        }
        else
        {
            // Auto generate standard template
            await ApplyStandardDeboningTemplateAsync();
        }

        RecalculateDeboningTotals();
        IsDeboningDialogOpen = true;
    }

    [RelayCommand]
    public void CloseDeboningDialog() => IsDeboningDialogOpen = false;

    // ==========================================
    // INDUSTRIAL SCALE READER COMMANDS (RS-232 / TCP-IP)
    // ==========================================
    [RelayCommand]
    public async Task ReadLiveAnimalScaleAsync()
    {
        try
        {
            var res = await _scaleService.ReadLiveWeightAsync("COM1", NewLiveWeightKg > 0 ? NewLiveWeightKg : 590m);
            NewLiveWeightKg = res.WeightKg;
            ShowStatus($"✓ Canlı Kantar Okundu ({res.ScalePortOrIp}): {res.WeightKg:N1} kg [Stabil: {res.IsStable}]", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Kantar okuma hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task ReadHotCarcassScaleAsync()
    {
        try
        {
            var res = await _scaleService.ReadLiveWeightAsync("COM2", HotCarcassWeightKg > 0 ? HotCarcassWeightKg : 325m);
            HotCarcassWeightKg = res.WeightKg;
            ColdCarcassWeightKg = Math.Round(res.WeightKg * 0.98m, 1);
            ShowStatus($"✓ Monoray Karkas Kantarı Okundu: {res.WeightKg:N1} kg", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Karkas kantar okuma hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task ReadDeboningBenchScaleAsync()
    {
        try
        {
            var res = await _scaleService.ReadLiveWeightAsync("COM3", NewCutWeightKg > 0 ? NewCutWeightKg : 14.5m);
            NewCutWeightKg = res.WeightKg;
            ShowStatus($"✓ Parçalama Tezgah Terazisi Okundu: {res.WeightKg:N2} kg", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Tezgah terazi okuma hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public void PrintDeboningCutLabel(CarcassDeboningCut? cut)
    {
        if (cut == null) return;

        try
        {
            var labelData = new Gs1MeatLabelData
            {
                CutName = cut.CutName,
                QualityGrade = cut.QualityGrade,
                ConformationClass = ConformationClass,
                EarTagNumber = cut.EarTagNumber,
                LotNumber = cut.LotNumber,
                NetWeightKg = cut.WeightKg,
                SlaughterDate = DateTime.UtcNow
            };

            var gs1Barcode = _labelService.GenerateGs1128BarcodeString(labelData);
            cut.Gs1Barcode128 = gs1Barcode;
            var zpl = _labelService.GenerateZplPrintCommand(labelData);

            ShowStatus($"✓ GS1-128 Barkod Etiketi Basıldı: {cut.CutName} ({cut.LotNumber}) - GS1: {gs1Barcode}", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Etiket yazdırma hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task ApplyStandardDeboningTemplateAsync()
    {
        try
        {
            var template = await _slaughterService.GenerateStandardDeboningTemplateAsync(
                DeboningCarcassNumber,
                DeboningTotalCarcassKg,
                SelectedAnimal?.AnimalType ?? AnimalType.Buyukbas);

            DeboningCuts.Clear();
            foreach (var cut in template)
            {
                DeboningCuts.Add(cut);
            }

            RecalculateDeboningTotals();
            ShowStatus($"✓ '{DeboningCarcassNumber}' için standart anatomik parçalama şablonu yüklendi.", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Şablon yükleme hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public void AddSingleCutToDeboningList()
    {
        if (string.IsNullOrWhiteSpace(NewCutName) || NewCutWeightKg <= 0)
        {
            ShowStatus("Lütfen geçerli bir et parça adı ve kilogramı girin.", "#DC2626");
            return;
        }

        int nextIdx = DeboningCuts.Count + 1;
        var yield = DeboningTotalCarcassKg > 0 ? Math.Round((NewCutWeightKg / DeboningTotalCarcassKg) * 100m, 2) : 0;
        
        var cut = new CarcassDeboningCut
        {
            SlaughterRecordId = SelectedSlaughterRecord?.Id ?? 0,
            CarcassNumber = DeboningCarcassNumber,
            EarTagNumber = DeboningEarTagNumber,
            AnatomicalRegion = NewCutAnatomicalRegion,
            CutName = NewCutName,
            WeightKg = NewCutWeightKg,
            YieldPercentage = yield,
            QualityGrade = NewCutQualityGrade,
            UnitCostEstimated = NewCutUnitCost,
            TotalCutValue = NewCutWeightKg * NewCutUnitCost,
            LotNumber = $"PRC-{DeboningCarcassNumber.Replace("KRK-", "")}-{nextIdx:D2}",
            Barcode = $"28{nextIdx:D2}{Math.Abs(DeboningCarcassNumber.GetHashCode()) % 10000:D4}{(int)(NewCutWeightKg * 100):D4}",
            TargetStorageLocation = NewCutStorageLocation,
            CutDate = DateTime.UtcNow,
            MasterButcher = "Usta Kasap"
        };

        DeboningCuts.Add(cut);
        RecalculateDeboningTotals();
        ShowStatus($"✓ {NewCutName} ({NewCutWeightKg:N1} kg) parçalama listesine eklendi.", "#16A34A");
    }

    [RelayCommand]
    public void RemoveCutFromDeboningList(CarcassDeboningCut? cut)
    {
        if (cut != null && DeboningCuts.Contains(cut))
        {
            DeboningCuts.Remove(cut);
            RecalculateDeboningTotals();
        }
    }

    [RelayCommand]
    public async Task SaveDeboningCutsAsync()
    {
        if (!DeboningCuts.Any())
        {
            ShowStatus("Kaydedilecek parçalama/kesim kaydı bulunamadı.", "#DC2626");
            return;
        }

        try
        {
            var saved = await _slaughterService.SaveDeboningCutsAsync(DeboningCarcassNumber, DeboningCuts.ToList());
            IsDeboningDialogOpen = false;
            ShowStatus($"✓ {DeboningCarcassNumber} nolu karkasın {saved.Count} adet anatomik et parçası ve lot barkodları stoklara kaydedildi!", "#16A34A");
            await LoadAllDataAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Parçalama kaydı hatası: {ex.Message}", "#DC2626");
        }
    }

    private void RecalculateDeboningTotals()
    {
        DeboningAssignedTotalKg = DeboningCuts.Sum(c => c.WeightKg);
        DeboningRemainingKg = DeboningTotalCarcassKg - DeboningAssignedTotalKg;
        DeboningEfficiencyYield = DeboningTotalCarcassKg > 0 ? Math.Round((DeboningAssignedTotalKg / DeboningTotalCarcassKg) * 100m, 1) : 0;
        DeboningTotalCutValue = DeboningCuts.Sum(c => c.TotalCutValue);

        // Update yield percentage on items

        foreach (var cut in DeboningCuts)
        {
            cut.YieldPercentage = DeboningTotalCarcassKg > 0 ? Math.Round((cut.WeightKg / DeboningTotalCarcassKg) * 100m, 2) : 0;
            cut.TotalCutValue = cut.WeightKg * cut.UnitCostEstimated;
        }
    }

    [RelayCommand]
    public async Task OpenPassportDialogAsync(AnimalIntakeDto? targetAnimal)
    {
        if (targetAnimal != null) SelectedAnimal = targetAnimal;
        var tag = SelectedAnimal?.EarTagNumber ?? "";
        if (string.IsNullOrEmpty(tag) && AnimalIntakes.Any())
        {
            tag = AnimalIntakes.First().EarTagNumber;
        }

        if (string.IsNullOrEmpty(tag))
        {
            ShowStatus("Lütfen pasaportu görüntülenecek bir hayvan seçin.", "#DC2626");
            return;
        }

        try
        {
            CurrentPassportDocument = await _passportPdfService.GetAnimalPassportDataAsync(tag);
            IsPassportDialogOpen = true;
            ShowStatus($"✓ Hayvan Geçmişi ve Pasaportu Yüklendi: {tag}", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Pasaport yükleme hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public void ClosePassportDialog() => IsPassportDialogOpen = false;

    [RelayCommand]
    public async Task ExportAndPrintPassportPdfAsync(string? earTag)
    {
        var tag = earTag ?? SelectedAnimal?.EarTagNumber ?? CurrentPassportDocument?.EarTagNumber ?? "";
        if (string.IsNullOrEmpty(tag) && AnimalIntakes.Any())
        {
            tag = AnimalIntakes.First().EarTagNumber;
        }

        if (string.IsNullOrEmpty(tag))
        {
            ShowStatus("Lütfen PDF çıktısı alınacak hayvanı seçin.", "#DC2626");
            return;
        }

        try
        {
            var path = await _passportPdfService.ExportAndOpenPassportPdfAsync(tag);
            ShowStatus($"✓ PDF / Yazdırma Belgesi Oluşturuldu: {System.IO.Path.GetFileName(path)}", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"PDF oluşturma hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task OpenTraceabilityModalAsync(AnimalIntakeDto? targetAnimal)
    {
        if (targetAnimal != null) SelectedAnimal = targetAnimal;
        var tag = SelectedAnimal?.EarTagNumber ?? "";
        if (string.IsNullOrEmpty(tag) && AnimalIntakes.Any())
        {
            tag = AnimalIntakes.First().EarTagNumber;
        }

        SearchTraceabilityQuery = tag;
        if (!string.IsNullOrEmpty(tag))
        {
            CurrentTraceTree = await _traceabilityService.GetTraceabilityTreeByEarTagAsync(tag);
        }
        IsTraceabilityModalOpen = true;
    }

    [RelayCommand]
    public void CloseTraceabilityModal() => IsTraceabilityModalOpen = false;

    [RelayCommand]
    public void OpenWasteModal() => IsWasteModalOpen = true;

    [RelayCommand]
    public void CloseWasteModal() => IsWasteModalOpen = false;

    [RelayCommand]
    public void OpenInvoiceModal() => IsInvoiceModalOpen = true;

    [RelayCommand]
    public void CloseInvoiceModal() => IsInvoiceModalOpen = false;

    // ==========================================
    // SAVE ACTIONS
    // ==========================================
    [RelayCommand]
    public async Task SaveAnimalIntakeAsync()
    {
        if (string.IsNullOrWhiteSpace(NewEarTagNumber))
        {
            ShowStatus("Lütfen hayvan küpe numarasını girin.", "#DC2626");
            return;
        }

        try
        {
            AnimalGender genderEnum = AnimalGender.Tosun;
            if (SelectedGenderString == "Düve") genderEnum = AnimalGender.Duve;
            else if (SelectedGenderString == "Dana") genderEnum = AnimalGender.Erkek;
            else if (SelectedGenderString == "İnek" || SelectedGenderString == "Koyun" || SelectedGenderString == "Keçi") genderEnum = AnimalGender.Disi;

            var intake = new AnimalIntake
            {
                EarTagNumber = NewEarTagNumber.Trim(),
                PassportNumber = NewPassportNumber.Trim(),
                AnimalType = SelectedAnimalType,
                Breed = SelectedBreed,
                Gender = genderEnum,
                AgeMonths = NewAgeMonths,
                LiveWeightKg = NewLiveWeightKg,
                PurchasePrice = NewPurchasePrice,
                ProducerName = NewProducerName.Trim(),
                WaybillNumber = NewWaybillNumber.Trim(),
                FarmOrigin = NewFarmOrigin.Trim(),
                RfidOrQrCode = NewRfidTag.Trim(),
                ArrivalDate = DateTime.UtcNow,
                BatchNumber = $"PRT-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 1000:D3}"
            };

            await _slaughterService.RegisterAnimalIntakeAsync(intake);
            IsAddAnimalModalOpen = false;
            ShowStatus($"✓ Küpe No: {NewEarTagNumber} başarıyla sisteme kabul edildi.", "#16A34A");
            await LoadAllDataAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Hayvan kaydı başarısız: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task SaveVeterinaryCheckAsync()
    {
        if (SelectedAnimal == null) return;

        try
        {
            var check = new VeterinaryCheck
            {
                AnimalIntakeId = SelectedAnimal.Id,
                VeterinarianName = VetDoctorName,
                CheckDate = DateTime.UtcNow,
                IsAntemortem = true,
                Status = SelectedVetStatus,
                IsApprovedForSlaughter = IsApprovedForSlaughter,
                BodyTemperature = VetBodyTemperature,
                Diagnosis = VetDiagnosis,
                ReportNumber = VetReportNumber
            };

            await _slaughterService.PerformVeterinaryCheckAsync(check);
            IsVetModalOpen = false;

            if (IsApprovedForSlaughter)
            {
                ShowStatus($"✓ Veteriner Muayenesi Onaylandı: {SelectedAnimal.EarTagNumber} kesime uygun.", "#16A34A");
            }
            else
            {
                ShowStatus($"⛔ KESİM BLOKE EDİLDİ: {SelectedAnimal.EarTagNumber} karantinaya alındı!", "#DC2626");
            }

            await LoadAllDataAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Veteriner kaydı hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task ExecuteSlaughterAsync()
    {
        if (SelectedAnimal == null) return;

        try
        {
            var record = new SlaughterRecord
            {
                AnimalIntakeId = SelectedAnimal.Id,
                ButcherPersonName = ButcherName,
                SlaughterLine = SlaughterLine,
                LiveWeightKg = SlaughterLiveWeightKg,
                HotCarcassWeightKg = HotCarcassWeightKg,
                ColdCarcassWeightKg = ColdCarcassWeightKg,
                HeadWeightKg = HeadWeightKg,
                HideWeightKg = HideWeightKg,
                OffalWeightKg = OffalWeightKg,
                FatWeightKg = FatWeightKg,
                BoneWeightKg = BoneWeightKg,
                SlaughterWasteKg = SlaughterWasteKg,
                AnimalPurchaseCost = AnimalPurchaseCost,
                SlaughterLaborCost = SlaughterLaborCost,
                TransportationCost = TransportationCost,
                CoolingElectricityCost = CoolingCost,
                GeneralOverheadCost = OverheadCost,
                ColdStorageLocation = "Soğuk Hava Odası #1 - Bölme A-04",
                HookRailNumber = HookRailNumber,
                ConformationClass = ConformationClass,
                FatCoverScore = FatCoverScore,
                MarblingScore = MarblingScore,
                PostMortemPh24 = PostMortemPh24
            };

            var res = await _slaughterService.ExecuteSlaughterAndWeighingAsync(record);
            IsSlaughterModalOpen = false;
            ShowStatus($"✓ KESİM VE TARTIM TAMAMLANDI: Karkas No {res.CarcassNumber} üretildi (Randıman: %{res.CarcassYieldPercentage:N1})", "#16A34A");
            await LoadAllDataAsync();

            // Automatically prompt deboning for this freshly slaughtered carcass
            var freshRecord = SlaughterRecords.FirstOrDefault(s => s.CarcassNumber == res.CarcassNumber);
            if (freshRecord != null)
            {
                await OpenDeboningDialogAsync(freshRecord);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Kesim işlemi engellendi: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task SearchTraceabilityAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTraceabilityQuery)) return;

        try
        {
            var res = await _traceabilityService.GetTraceabilityTreeByEarTagAsync(SearchTraceabilityQuery)
                      ?? await _traceabilityService.GetTraceabilityTreeByBarcodeOrLotAsync(SearchTraceabilityQuery);

            if (res != null)
            {
                CurrentTraceTree = res;
                ShowStatus($"✓ İzlenebilirlik Zinciri Bulundu: {res.EarTagNumber} - {res.CarcassNumber}", "#16A34A");
            }
            else
            {
                ShowStatus($"'{SearchTraceabilityQuery}' ile eşleşen küpe veya karkas kaydı bulunamadı.", "#DC2626");
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Arama hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task SaveWasteLogAsync()
    {
        try
        {
            var waste = new WasteLog
            {
                CauseType = NewWasteCause,
                ProductName = NewWasteProductName,
                LotOrCarcassNumber = NewWasteLot,
                WeightKg = NewWasteWeightKg,
                UnitCost = NewWasteCostPerKg,
                ResponsiblePerson = "Nöbetçi Kasap",
                ApprovedByPerson = "Yönetici (Admin)",
                Description = NewWasteReason,
                LogDate = DateTime.UtcNow
            };

            await _slaughterService.LogWasteAsync(waste);
            IsWasteModalOpen = false;
            ShowStatus($"✓ Zayiat / Fire Kaydedildi: {NewWasteWeightKg:N2} kg ({NewWasteWeightKg * NewWasteCostPerKg:C2})", "#16A34A");
            await LoadAllDataAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Zayiat kaydı hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task CalculateTrueCostAsync()
    {
        try
        {
            if (SelectedSlaughterRecord == null)
            {
                ShowStatus("Lütfen maliyeti hesaplanacak bir karkas kesim kaydı seçin.", "#DC2626");
                return;
            }

            CurrentTrueCostRollup = await _trueCostService.CalculateTrueCostAsync(SelectedSlaughterRecord.CarcassNumber);
            ShowStatus($"✓ Gerçek Maliyet Hesaplandı: Net Karkas Kg Başı Maliyet: {CurrentTrueCostRollup.TrueCostPerCarcassKg:C2} (Deri & Sakatat Geliri Düşüldü)", "#16A34A");
        }
        catch (Exception ex)
        {
            ShowStatus($"Maliyet hesaplama hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task OpenTrueCostDialogAsync(SlaughterRecordDto? record)
    {
        if (record != null) SelectedSlaughterRecord = record;
        if (SelectedSlaughterRecord == null && SlaughterRecords.Any()) SelectedSlaughterRecord = SlaughterRecords.First();

        if (SelectedSlaughterRecord != null)
        {
            await CalculateTrueCostAsync();
            IsTrueCostDialogOpen = true;
        }
        else
        {
            ShowStatus("Maliyet hesabı için karkas kesim kaydı bulunamadı.", "#DC2626");
        }
    }

    [RelayCommand]
    public void CloseTrueCostDialog()
    {
        IsTrueCostDialogOpen = false;
    }

    [RelayCommand]
    public void OpenRecallDialog(AnimalIntakeDto? animal)
    {
        if (animal != null) SelectedAnimal = animal;
        IsRecallDialogOpen = true;
    }

    [RelayCommand]
    public void CloseRecallDialog()
    {
        IsRecallDialogOpen = false;
    }

    [RelayCommand]
    public async Task TriggerEmergencyRecallAsync(string reason = "Bakanlık Resmi Gıda Güvenliği Alarmı")
    {
        try
        {
            var query = !string.IsNullOrEmpty(SearchTraceabilityQuery) 
                ? SearchTraceabilityQuery 
                : (SelectedAnimal?.EarTagNumber ?? SelectedSlaughterRecord?.CarcassNumber ?? "");

            if (string.IsNullOrWhiteSpace(query))
            {
                ShowStatus("Geri çağırma için küpe veya karkas numarası giriniz.", "#DC2626");
                return;
            }

            LastRecallResult = await _recallService.Trigger1ClickRecallAsync(query, reason);
            ShowStatus($"🚨 ACİL GERİ ÇAĞIRMA BAŞLATILDI: {LastRecallResult.CaseNumber} ({LastRecallResult.AffectedLotsCount} Lot, {LastRecallResult.TotalRecalledWeightKg:N2} kg Bloke Edildi)", "#DC2626");
            await LoadAllDataAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Geri çağırma hatası: {ex.Message}", "#DC2626");
        }
    }

    [RelayCommand]
    public async Task ScanAnomaliesAsync()
    {
        try
        {
            var list = await _anomalyService.ScanAndDetectAnomaliesAsync();
            Anomalies.Clear();
            foreach (var a in list) Anomalies.Add(a);
            ShowStatus($"✓ Adli Tarama Tamamlandı: {list.Count} adet kritik operasyonel anomali tespit edildi.", "#D97706");
        }
        catch (Exception ex)
        {
            ShowStatus($"Tarama hatası: {ex.Message}", "#DC2626");
        }
    }

    private void ShowStatus(string msg, string color)
    {
        StatusMessage = msg;
        StatusColor = color;
    }

    partial void OnHotCarcassWeightKgChanged(decimal value)
    {
        ColdCarcassWeightKg = Math.Round(value * 0.98m, 2);
        OnPropertyChanged(nameof(CalculatedYieldPercentage));
        OnPropertyChanged(nameof(CalculatedCostPerKg));
        OnPropertyChanged(nameof(CalculatedTotalCost));
    }

    partial void OnSlaughterLiveWeightKgChanged(decimal value)
    {
        OnPropertyChanged(nameof(CalculatedYieldPercentage));
    }

    partial void OnSearchFilterTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedAnimalTypeChanged(AnimalType value)
    {
        _ = RefreshBreedsForTypeAsync(value);
    }

    private async Task RefreshBreedsForTypeAsync(AnimalType type)
    {
        try
        {
            var breeds = await _slaughterService.GetAllBreedsAsync(type);
            AvailableBreeds.Clear();
            foreach (var b in breeds) AvailableBreeds.Add(b);
            if (AvailableBreeds.Any())
            {
                SelectedBreed = AvailableBreeds.First();
            }
        }
        catch { }
    }
}
