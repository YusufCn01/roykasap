using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.WPF.Services;

namespace KasapOtomasyon.WPF.ViewModels;

public partial class ShelfLifeReprocessingViewModel : ObservableObject
{
    private readonly IShelfLifeReprocessingService _reprocessingService;
    private readonly ILocalizationService _locService;
    private readonly INavigationService _navService;

    // Batches
    [ObservableProperty]
    private ObservableCollection<ExpiringBatchDto> expiringBatches = new();

    [ObservableProperty]
    private ExpiringBatchDto? selectedBatch;

    // Recipes
    [ObservableProperty]
    private ObservableCollection<ProcessingRecipeDto> recipes = new();

    [ObservableProperty]
    private ProcessingRecipeDto? selectedRecipe;

    // Simulation & Transformation
    [ObservableProperty]
    private decimal rawMeatQuantityKg = 15.0m;

    [ObservableProperty]
    private ReprocessingSimulationResultDto? currentSimulation;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    // --- Modal: Update Shelf Display Expiry Date ---
    [ObservableProperty]
    private bool isShelfDateModalOpen;

    [ObservableProperty]
    private ExpiringBatchDto? targetBatchForDateEdit;

    [ObservableProperty]
    private DateTime editShelfDate = DateTime.Today.AddDays(2);

    [ObservableProperty]
    private string editShelfReason = string.Empty;

    // --- Modal: Recipe Definition & Edit ---
    [ObservableProperty]
    private bool isRecipeEditorModalOpen;

    [ObservableProperty]
    private int editRecipeId;

    [ObservableProperty]
    private string editRecipeCode = string.Empty;

    [ObservableProperty]
    private string editRecipeName = string.Empty;

    [ObservableProperty]
    private RecipeProductCategory editRecipeCategory = RecipeProductCategory.FreshSausage;

    [ObservableProperty]
    private string editTargetProductName = string.Empty;

    [ObservableProperty]
    private decimal editStandardBatchWeightKg = 100.0m;

    [ObservableProperty]
    private decimal editExpectedYieldPercentage = 92.0m;

    [ObservableProperty]
    private int editShelfLifeExtensionDays = 45;

    [ObservableProperty]
    private string editDescription = string.Empty;

    [ObservableProperty]
    private string editProcessInstructions = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CustomRecipeItemInputDto> editRecipeItems = new();

    // New item inputs for recipe builder
    [ObservableProperty]
    private RecipeItemType newIngredientType = RecipeItemType.SpiceMix;

    [ObservableProperty]
    private string newIngredientName = string.Empty;

    [ObservableProperty]
    private decimal newIngredientPercentage = 5.0m;

    [ObservableProperty]
    private decimal newIngredientUnitCost = 80.0m;

    [ObservableProperty]
    private string newIngredientDescription = string.Empty;

    // --- Navigation Tabs ---
    [ObservableProperty]
    private int selectedTabIndex = 0;

    // --- Industrial Butchery Equipment Park ---
    [ObservableProperty]
    private ObservableCollection<MeatProcessingEquipmentDto> equipments = new();

    [ObservableProperty]
    private MeatProcessingEquipmentDto? selectedEquipment;

    // --- Auxiliary Materials & Spices/Sauces/Casings ---
    [ObservableProperty]
    private ObservableCollection<AuxiliaryMaterialStockDto> auxiliaryStocks = new();

    [ObservableProperty]
    private AuxiliaryMaterialStockDto? selectedAuxiliaryStock;

    [ObservableProperty]
    private decimal auxStockDelta = 5.0m;

    [ObservableProperty]
    private string auxStockReason = "Depo İkmal / Rutin Sayım";

    // --- Quality, pH, Aw & HACCP Control ---
    [ObservableProperty]
    private ObservableCollection<ReprocessingQualityCheckDto> qualityChecks = new();

    [ObservableProperty]
    private string newQcBatchLot = string.Empty;

    [ObservableProperty]
    private string newQcProductName = string.Empty;

    [ObservableProperty]
    private decimal newQcPh = 5.65m;

    [ObservableProperty]
    private decimal newQcAw = 0.88m;

    [ObservableProperty]
    private decimal newQcTemp = 2.8m;

    [ObservableProperty]
    private string newQcInspector = "Dr. Vet. Mehmet Demir";

    [ObservableProperty]
    private string newQcNotes = string.Empty;

    // --- Financial Salvage & Waste ROI ---
    [ObservableProperty]
    private FinancialRoiSummaryDto? roiSummary;

    // --- 1. Sucuk Curing & Drying Chamber (IoT) ---
    [ObservableProperty]
    private ObservableCollection<SucukCuringBatchDto> curingBatches = new();

    [ObservableProperty]
    private SucukCuringBatchDto? selectedCuringBatch;

    [ObservableProperty]
    private decimal newCuringWeightKg = 78.5m;

    [ObservableProperty]
    private decimal newCuringPh = 5.15m;

    [ObservableProperty]
    private decimal newCuringTemp = 15.2m;

    [ObservableProperty]
    private decimal newCuringHumidity = 78.0m;

    [ObservableProperty]
    private string curingMeasurementNotes = "Rutin IoT sensör ve kantar okuması.";

    // --- 2. Organoleptic & Sensory Checks ---
    [ObservableProperty]
    private ObservableCollection<OrganolepticSensoryCheckDto> sensoryChecks = new();

    [ObservableProperty]
    private string sensoryBatchLot = string.Empty;

    [ObservableProperty]
    private string sensoryProductName = string.Empty;

    [ObservableProperty]
    private string sensoryInspector = "Kıdemli Kasap Şefi";

    [ObservableProperty]
    private int sensorySmellScore = 5;

    [ObservableProperty]
    private int sensoryTextureScore = 4;

    [ObservableProperty]
    private int sensoryColorScore = 5;

    [ObservableProperty]
    private string sensoryNotes = "Doğal et rengi ve taze aromatik yapı.";

    // --- 3. Official Disposal & Rendering Records ---
    [ObservableProperty]
    private ObservableCollection<ReprocessingDisposalRecordDto> disposalRecords = new();

    [ObservableProperty]
    private decimal disposalQuantityKg = 12.5m;

    [ObservableProperty]
    private string disposalReason = "pH > 6.20 veya reyon bekleme süresi aşımı nedeniyle imha.";

    [ObservableProperty]
    private string disposalHaccpOfficer = "Dr. Vet. Mehmet Demir (HACCP)";

    [ObservableProperty]
    private string disposalButcherChief = "Usta Kasap Ahmet Yılmaz";

    [ObservableProperty]
    private string disposalRenderingCompany = "Biyo-Atık & Rendering A.Ş.";

    [ObservableProperty]
    private string disposalWaybillNo = "IRS-2026-8841";

    // --- 4. Butcher Waste Prevention Incentives ---
    [ObservableProperty]
    private ObservableCollection<ButcherWasteIncentiveDto> butcherIncentives = new();

    // --- 5. Legal Thermal Label Generator ---
    [ObservableProperty]
    private LegalThermalLabelDto? thermalLabelPreview;

    [ObservableProperty]
    private bool isThermalLabelModalOpen;

    // --- 6. Smart Markdown & Dynamic Pricing ---
    [ObservableProperty]
    private SmartMarkdownResultDto? smartMarkdownResult;

    [ObservableProperty]
    private decimal customMarkdownPercent = 20.0m;

    // --- 7. Fast Shelf Barcode Scanner ---
    [ObservableProperty]
    private string fastScanInput = string.Empty;

    [ObservableProperty]
    private FastScanBatchResultDto? fastScanResult;

    // Supported Enums for UI bindings
    public IEnumerable<RecipeProductCategory> RecipeCategories => Enum.GetValues<RecipeProductCategory>();
    public IEnumerable<RecipeItemType> RecipeItemTypes => Enum.GetValues<RecipeItemType>();

    // Localized Strings (dynamic access)
    public string Loc(string key, params object[] args) => _locService.Get(key, args);
    public string CurrentLanguageCode => _locService.CurrentLanguageCode;
    public string CurrentLanguageFlag => _locService.CurrentLanguage.FlagEmoji;
    public string CurrentLanguageName => _locService.CurrentLanguage.DisplayName;

    // Localized Quick Properties
    public string LTitle => Loc("reprocessing.title");
    public string LSubtitle => Loc("reprocessing.subtitle");
    public string LBatchesTitle => Loc("reprocessing.batches.title");
    public string LBatchesDesc => Loc("reprocessing.batches.desc");
    public string LRecipesTitle => Loc("reprocessing.recipes.title");
    public string LRecipesDesc => Loc("reprocessing.recipes.desc");
    public string LSimTitle => Loc("reprocessing.simulation.title");
    public string LSimDesc => Loc("reprocessing.simulation.desc");
    public string LHaccpTitle => Loc("reprocessing.haccp.whyCuring");
    public string LHaccpText => Loc("reprocessing.haccp.whyCuringText");

    // Tab Headers
    public string LTabOverview => Loc("reprocessing.tab.overview");
    public string LTabQc => Loc("reprocessing.tab.qc");
    public string LTabAux => Loc("reprocessing.tab.aux");
    public string LTabEquipment => Loc("reprocessing.tab.equipment");
    public string LTabRoi => Loc("reprocessing.tab.roi");
    public string LTabCuring => Loc("reprocessing.tab.curing");
    public string LTabSensory => Loc("reprocessing.tab.sensory");
    public string LTabDisposal => Loc("reprocessing.tab.disposal");
    public string LTabIncentives => Loc("reprocessing.tab.incentives");
    public string LTabLabel => Loc("reprocessing.tab.label");
    public string LMarkdownApply => Loc("markdown.apply");
    public string LFastScanTitle => Loc("fastscan.title");

    public ShelfLifeReprocessingViewModel(
        IShelfLifeReprocessingService reprocessingService,
        ILocalizationService locService,
        INavigationService navService)
    {
        _reprocessingService = reprocessingService;
        _locService = locService;
        _navService = navService;

        _locService.LanguageChanged += (s, lang) =>
        {
            OnPropertyChanged(string.Empty); // Refresh all data bindings
            _ = LoadDataAsync();
        };

        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var batches = await _reprocessingService.GetExpiringBatchesAsync(thresholdDays: 3);
            ExpiringBatches = new ObservableCollection<ExpiringBatchDto>(batches);

            if (SelectedBatch == null || !batches.Any(b => b.StockItemId == SelectedBatch.StockItemId))
            {
                SelectedBatch = batches.FirstOrDefault();
            }

            var recs = await _reprocessingService.GetAllRecipesAsync();
            Recipes = new ObservableCollection<ProcessingRecipeDto>(recs);

            if (SelectedRecipe == null || !recs.Any(r => r.Id == SelectedRecipe.Id))
            {
                SelectedRecipe = recs.FirstOrDefault();
            }

            if (SelectedBatch != null)
            {
                RawMeatQuantityKg = SelectedBatch.CurrentQuantityKg;
            }

            await RecalculateSimulationAsync();

            var eqs = await _reprocessingService.GetEquipmentsAsync();
            Equipments = new ObservableCollection<MeatProcessingEquipmentDto>(eqs);
            SelectedEquipment = eqs.FirstOrDefault();

            var aux = await _reprocessingService.GetAuxiliaryMaterialStocksAsync();
            AuxiliaryStocks = new ObservableCollection<AuxiliaryMaterialStockDto>(aux);
            SelectedAuxiliaryStock = aux.FirstOrDefault();

            var qcs = await _reprocessingService.GetQualityChecksAsync();
            QualityChecks = new ObservableCollection<ReprocessingQualityCheckDto>(qcs);

            RoiSummary = await _reprocessingService.GetFinancialRoiSummaryAsync();

            var curings = await _reprocessingService.GetCuringBatchesAsync();
            CuringBatches = new ObservableCollection<SucukCuringBatchDto>(curings);
            SelectedCuringBatch = curings.FirstOrDefault();
            if (SelectedCuringBatch != null)
            {
                NewCuringWeightKg = SelectedCuringBatch.CurrentWeightKg;
                NewCuringPh = SelectedCuringBatch.CurrentPh;
                NewCuringTemp = SelectedCuringBatch.ChamberTemperatureCelsius;
                NewCuringHumidity = SelectedCuringBatch.ChamberHumidityRh;
            }

            var sensory = await _reprocessingService.GetSensoryChecksAsync();
            SensoryChecks = new ObservableCollection<OrganolepticSensoryCheckDto>(sensory);

            var disposals = await _reprocessingService.GetDisposalRecordsAsync();
            DisposalRecords = new ObservableCollection<ReprocessingDisposalRecordDto>(disposals);

            var incentives = await _reprocessingService.GetButcherIncentivesAsync();
            ButcherIncentives = new ObservableCollection<ButcherWasteIncentiveDto>(incentives);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SelectBatch(ExpiringBatchDto? batch)
    {
        SelectedBatch = batch;
        if (batch != null)
        {
            RawMeatQuantityKg = batch.CurrentQuantityKg;
            await RecalculateSimulationAsync();
        }
    }

    [RelayCommand]
    public async Task SelectRecipe(ProcessingRecipeDto? recipe)
    {
        SelectedRecipe = recipe;
        await RecalculateSimulationAsync();
    }

    [RelayCommand]
    public async Task RecalculateSimulationAsync()
    {
        if (SelectedBatch == null || SelectedRecipe == null || RawMeatQuantityKg <= 0)
        {
            CurrentSimulation = null;
            return;
        }

        try
        {
            CurrentSimulation = await _reprocessingService.SimulateReprocessingAsync(
                SelectedBatch.StockItemId,
                SelectedRecipe.Id,
                RawMeatQuantityKg);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Simülasyon hatası: {ex.Message}";
        }
    }

    // --- Shelf Date Modal Commands ---
    [RelayCommand]
    public void OpenShelfDateModal(ExpiringBatchDto? batch)
    {
        TargetBatchForDateEdit = batch ?? SelectedBatch;
        if (TargetBatchForDateEdit != null)
        {
            EditShelfDate = TargetBatchForDateEdit.ShelfDisplayExpiryDate;
            EditShelfReason = Loc("reprocessing.modal.shelfDate.reasonHint");
            IsShelfDateModalOpen = true;
        }
    }

    [RelayCommand]
    public void CloseShelfDateModal()
    {
        IsShelfDateModalOpen = false;
    }

    [RelayCommand]
    public async Task SaveShelfDateAsync()
    {
        if (TargetBatchForDateEdit == null) return;

        try
        {
            var success = await _reprocessingService.UpdateShelfDisplayExpiryDateAsync(
                TargetBatchForDateEdit.StockItemId,
                EditShelfDate,
                EditShelfReason,
                "Reyon Şefi (Yetkili)");

            if (success)
            {
                IsShelfDateModalOpen = false;
                StatusMessage = $"✓ {TargetBatchForDateEdit.ProductName} Reyon SKT {EditShelfDate:dd.MM.yyyy} olarak güncellendi.";
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tarih güncellenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Recipe Editor Modal Commands ---
    [RelayCommand]
    public void OpenNewRecipeModal()
    {
        EditRecipeId = 0;
        EditRecipeCode = $"RCP-OZEL-{new Random().Next(100, 999)}";
        EditRecipeName = string.Empty;
        EditRecipeCategory = RecipeProductCategory.FreshSausage;
        EditTargetProductName = string.Empty;
        EditStandardBatchWeightKg = 100.0m;
        EditExpectedYieldPercentage = 95.0m;
        EditShelfLifeExtensionDays = 14;
        EditDescription = string.Empty;
        EditProcessInstructions = string.Empty;

        EditRecipeItems = new ObservableCollection<CustomRecipeItemInputDto>
        {
            new() { ItemType = RecipeItemType.MeatRawMaterial, ItemName = "Ana Et Hammaddesi", PercentageRatio = 75.0m, StandardUnitCost = 400.0m, Description = "Reyondan aktarılacak et" },
            new() { ItemType = RecipeItemType.FatRawMaterial, ItemName = "Kavram / Kuyruk Yağı", PercentageRatio = 15.0m, StandardUnitCost = 160.0m, Description = "Lezzet ve nem verici yağ" },
            new() { ItemType = RecipeItemType.SpiceMix, ItemName = "Özel Baharat Harcı", PercentageRatio = 5.0m, StandardUnitCost = 120.0m, Description = "Doğal aroma ve koruyucu" },
            new() { ItemType = RecipeItemType.SaltAndCuring, ItemName = "Kaya Tuzu", PercentageRatio = 2.0m, StandardUnitCost = 25.0m, Description = "Su aktivitesini düşürücü tuz" },
            new() { ItemType = RecipeItemType.SauceAndMarinade, ItemName = "Özel Sos / Sıvı Marinat", PercentageRatio = 3.0m, StandardUnitCost = 90.0m, Description = "Marinasyon sosu" }
        };

        IsRecipeEditorModalOpen = true;
    }

    [RelayCommand]
    public void OpenEditRecipeModal(ProcessingRecipeDto? recipe)
    {
        var target = recipe ?? SelectedRecipe;
        if (target == null) return;

        EditRecipeId = target.Id;
        EditRecipeCode = target.RecipeCode;
        EditRecipeName = target.RecipeName;
        EditRecipeCategory = target.Category;
        EditTargetProductName = target.TargetProductName;
        EditStandardBatchWeightKg = target.StandardBatchWeightKg;
        EditExpectedYieldPercentage = target.ExpectedYieldPercentage;
        EditShelfLifeExtensionDays = target.ShelfLifeExtensionDays;
        EditDescription = target.Description;
        EditProcessInstructions = target.ProcessInstructions;

        EditRecipeItems = new ObservableCollection<CustomRecipeItemInputDto>(
            target.Items.Select(i => new CustomRecipeItemInputDto
            {
                Id = i.Id,
                ItemType = i.ItemType,
                ItemName = i.ItemName,
                PercentageRatio = i.PercentageRatio,
                StandardUnitCost = i.StandardUnitCost,
                Description = i.Description
            }));

        IsRecipeEditorModalOpen = true;
    }

    [RelayCommand]
    public void CloseRecipeEditorModal()
    {
        IsRecipeEditorModalOpen = false;
    }

    [RelayCommand]
    public void AddIngredientToEditRecipe()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientName))
        {
            MessageBox.Show("Bileşen adını giriniz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        EditRecipeItems.Add(new CustomRecipeItemInputDto
        {
            ItemType = NewIngredientType,
            ItemName = NewIngredientName.Trim(),
            PercentageRatio = NewIngredientPercentage,
            StandardUnitCost = NewIngredientUnitCost,
            Description = NewIngredientDescription.Trim()
        });

        NewIngredientName = string.Empty;
        NewIngredientDescription = string.Empty;
    }

    [RelayCommand]
    public void RemoveIngredient(CustomRecipeItemInputDto? item)
    {
        if (item != null)
        {
            EditRecipeItems.Remove(item);
        }
    }

    [RelayCommand]
    public async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(EditRecipeName))
        {
            MessageBox.Show("Lütfen reçete adını giriniz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var totalRatio = EditRecipeItems.Sum(i => i.PercentageRatio);
        if (Math.Abs(totalRatio - 100.0m) > 5.0m)
        {
            var res = MessageBox.Show(
                $"Bileşen oranları toplamı %{totalRatio:N1} (Genellikle %100 olması önerilir). Yine de kaydetmek istiyor musunuz?",
                "Oran Uyarısı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
        }

        try
        {
            var dto = new CustomRecipeInputDto
            {
                Id = EditRecipeId,
                RecipeCode = EditRecipeCode,
                RecipeName = EditRecipeName,
                Category = EditRecipeCategory,
                TargetProductName = string.IsNullOrWhiteSpace(EditTargetProductName) ? EditRecipeName : EditTargetProductName,
                StandardBatchWeightKg = EditStandardBatchWeightKg,
                ExpectedYieldPercentage = EditExpectedYieldPercentage,
                ShelfLifeExtensionDays = EditShelfLifeExtensionDays,
                Description = EditDescription,
                ProcessInstructions = EditProcessInstructions,
                Items = EditRecipeItems.ToList()
            };

            var saved = await _reprocessingService.SaveCustomRecipeAsync(dto);
            IsRecipeEditorModalOpen = false;
            StatusMessage = $"✓ Reçete kaydedildi: {saved.RecipeName}";
            
            await LoadDataAsync();
            SelectedRecipe = Recipes.FirstOrDefault(r => r.Id == saved.Id);
            await RecalculateSimulationAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Reçete kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task DeleteRecipeAsync(ProcessingRecipeDto? recipe)
    {
        var target = recipe ?? SelectedRecipe;
        if (target == null) return;

        if (MessageBox.Show($"'{target.RecipeName}' reçetesini silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            var success = await _reprocessingService.DeleteRecipeAsync(target.Id);
            if (success)
            {
                StatusMessage = $"Reçete silindi: {target.RecipeName}";
                await LoadDataAsync();
            }
        }
    }

    // --- Reprocessing Execution Command ---
    [RelayCommand]
    public async Task ExecuteReprocessingAsync()
    {
        if (SelectedBatch == null)
        {
            MessageBox.Show("Lütfen dönüştürülecek et partisini seçin.", "Parti Seçilmedi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedRecipe == null)
        {
            MessageBox.Show("Lütfen uygulanacak ürün reçetesini seçin.", "Reçete Seçilmedi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (RawMeatQuantityKg <= 0 || RawMeatQuantityKg > SelectedBatch.CurrentQuantityKg)
        {
            MessageBox.Show($"Geçerli bir et miktarı giriniz (Mevcut stok: {SelectedBatch.CurrentQuantityKg:N2} kg).", "Geçersiz Miktar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmMsg = $"{SelectedBatch.ProductName} partisinden {RawMeatQuantityKg:N2} kg et kullanılarak;\n\n" +
                         $"Reçete: {SelectedRecipe.RecipeName}\n" +
                         $"Beklenen Mamul: {CurrentSimulation?.OutputProductKg:N2} kg\n" +
                         $"Yeni SKT: {CurrentSimulation?.NewCalculatedExpiryDate:dd.MM.yyyy} (+{SelectedRecipe.ShelfLifeExtensionDays} gün)\n\n" +
                         $"Dönüştürme işlemini onaylıyor musunuz?";

        if (MessageBox.Show(confirmMsg, "Dönüşüm Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var request = new ExecuteReprocessingRequestDto
            {
                StockItemId = SelectedBatch.StockItemId,
                RecipeId = SelectedRecipe.Id,
                RawMeatUsedKg = RawMeatQuantityKg,
                OperatorName = "Usta Kasap (Şarküteri Şefi)",
                CustomNotes = $"Reyon SKT yaklaşması nedeniyle {SelectedRecipe.RecipeName} üretimine aktarıldı."
            };

            var result = await _reprocessingService.ExecuteReprocessingAsync(request);
            if (result.Success)
            {
                StatusMessage = result.Message;
                MessageBox.Show(
                    $"DÖNÜŞÜM BAŞARIYLA TAMAMLANDI!\n\n" +
                    $"Üretilen Mamul: {result.OutputProductName}\n" +
                    $"Net Miktar: {result.OutputWeightKg:N2} kg\n" +
                    $"Yeni Parti No: {result.GeneratedLotNumber}\n" +
                    $"Yeni Son Kullanım Tarihi: {result.NewExpiryDate:dd.MM.yyyy}\n" +
                    $"Yeni Birim Maliyet: {result.NewUnitCost:N2} TL/kg\n" +
                    $"Üretim Barkodu: {result.GeneratedBarcode}\n\n" +
                    $"Hammadde reyon stokundan düşülmüş ve yeni mamul şarküteri stoklarına alınmıştır.",
                    "İşlem Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show($"Dönüşüm başarısız: {result.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Dönüşüm işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void SwitchLanguage(string languageCode)
    {
        _locService.SetLanguage(languageCode);
    }

    [RelayCommand]
    public void SetTab(string tabIndexStr)
    {
        if (int.TryParse(tabIndexStr, out int idx))
        {
            SelectedTabIndex = idx;
        }
    }

    [RelayCommand]
    public async Task MarkEquipmentSanitized(MeatProcessingEquipmentDto? eq)
    {
        var target = eq ?? SelectedEquipment;
        if (target == null) return;

        var success = await _reprocessingService.UpdateEquipmentSanitizationAsync(
            target.Id,
            "Usta Kasap",
            "Perasetik Asit (%0.2) + 82°C Sıcak Su",
            true);

        if (success)
        {
            StatusMessage = $"{target.EquipmentName} dezenfeksiyonu tamamlandı ve 'Hazır' olarak işaretlendi.";
            var eqs = await _reprocessingService.GetEquipmentsAsync();
            Equipments = new ObservableCollection<MeatProcessingEquipmentDto>(eqs);
            SelectedEquipment = Equipments.FirstOrDefault(e => e.Id == target.Id);
        }
    }

    [RelayCommand]
    public async Task AdjustAuxStock(AuxiliaryMaterialStockDto? item)
    {
        var target = item ?? SelectedAuxiliaryStock;
        if (target == null) return;

        if (AuxStockDelta == 0) return;

        var success = await _reprocessingService.AdjustAuxiliaryMaterialStockAsync(
            target.Id,
            AuxStockDelta,
            AuxStockReason);

        if (success)
        {
            StatusMessage = $"{target.MaterialName} stoğu güncellendi ({AuxStockDelta:+0.##;-0.##} {target.UnitOfMeasure}).";
            var aux = await _reprocessingService.GetAuxiliaryMaterialStocksAsync();
            AuxiliaryStocks = new ObservableCollection<AuxiliaryMaterialStockDto>(aux);
            SelectedAuxiliaryStock = AuxiliaryStocks.FirstOrDefault(a => a.Id == target.Id);
        }
    }

    [RelayCommand]
    public async Task RecordNewQualityCheckAsync()
    {
        try
        {
            var req = new RecordQualityCheckRequestDto
            {
                BatchLotNumber = !string.IsNullOrWhiteSpace(NewQcBatchLot) 
                    ? NewQcBatchLot 
                    : (SelectedBatch?.LotNumber ?? $"LOT-{DateTime.UtcNow:yyyyMMdd}"),
                ProductName = !string.IsNullOrWhiteSpace(NewQcProductName) 
                    ? NewQcProductName 
                    : (SelectedBatch?.ProductName ?? "Dana Kuşbaşı"),
                MeasuredPh = NewQcPh,
                MeasuredWaterActivityAw = NewQcAw,
                MeasuredCoreTempCelsius = NewQcTemp,
                InspectorName = NewQcInspector,
                Notes = NewQcNotes
            };

            var qc = await _reprocessingService.RecordQualityCheckAsync(req);
            
            var qcs = await _reprocessingService.GetQualityChecksAsync();
            QualityChecks = new ObservableCollection<ReprocessingQualityCheckDto>(qcs);

            if (qc.IsAutoBlocked)
            {
                MessageBox.Show(
                    $"DİKKAT: Kritik pH Limiti Aşıldı (pH: {qc.MeasuredPh:N2} > 6.20)!\n\n" +
                    $"Bu parti için mikrobiyal bozulma riski nedeniyle DÖNÜŞÜM YASAKLANDI ve parti bloke edildi.\n" +
                    $"Prosedür gereği imha işlemi başlatılmalıdır.",
                    "HACCP Kritik Limit Aşımı (pH > 6.20)",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    $"Gıda güvenliği ve proses kontrolü başarıyla kaydedildi.\n\n" +
                    $"Parti: {qc.BatchLotNumber}\n" +
                    $"pH: {qc.MeasuredPh:N2} (Normal Güvenli Aralık)\n" +
                    $"Su Aktivitesi (Aw): {qc.MeasuredWaterActivityAw:N2}\n" +
                    $"Çekirdek Sıcaklığı: {qc.MeasuredCoreTempCelsius:N1}°C\n" +
                    $"Karar: {qc.QualityVerdict}",
                    "Kalite & pH Kontrolü Onaylandı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kalite kontrolü kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task RefreshRoiSummaryAsync()
    {
        RoiSummary = await _reprocessingService.GetFinancialRoiSummaryAsync();
    }

    // --- Thermal Label Commands ---
    [RelayCommand]
    public async Task GenerateThermalLabelAsync()
    {
        if (SelectedBatch == null)
        {
            MessageBox.Show("Lütfen etiket oluşturmak için bir parti seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            ThermalLabelPreview = await _reprocessingService.GenerateThermalLabelAsync(
                SelectedBatch.StockItemId,
                SelectedRecipe?.Id);
            IsThermalLabelModalOpen = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Etiket oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void CloseThermalLabelModal()
    {
        IsThermalLabelModalOpen = false;
    }

    // --- Smart Markdown Dynamic Pricing Command ---
    [RelayCommand]
    public async Task ApplySmartMarkdownAsync()
    {
        if (SelectedBatch == null)
        {
            MessageBox.Show("Lütfen indirim uygulanacak partiyi seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var req = new SmartMarkdownRequestDto
            {
                StockItemId = SelectedBatch.StockItemId,
                DiscountPercentage = CustomMarkdownPercent,
                MarkdownReason = "Reyon SKT kritik seviyeye yaklaştığı için dinamik akıllı kademeli indirim.",
                OperatorName = "Reyon Sorumlusu"
            };

            SmartMarkdownResult = await _reprocessingService.ApplySmartMarkdownAsync(req);
            StatusMessage = $"✓ Akıllı indirim uygulandı: {SmartMarkdownResult.ProductName} -> {SmartMarkdownResult.NewDiscountedPriceLira:N2} TL (%{SmartMarkdownResult.DiscountPercentage:N0} indirim)";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Akıllı indirim uygulanırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Fast Barcode Scanner Command ---
    [RelayCommand]
    public async Task ExecuteFastScanAsync()
    {
        if (string.IsNullOrWhiteSpace(FastScanInput)) return;

        try
        {
            FastScanResult = await _reprocessingService.ScanBatchBarcodeAsync(FastScanInput.Trim());
            if (FastScanResult != null)
            {
                StatusMessage = $"Barkod okundu: {FastScanResult.ProductName} - {FastScanResult.RecommendedAction}";
                var matched = ExpiringBatches.FirstOrDefault(b => b.LotNumber.Equals(FastScanResult.BarcodeOrLot, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    SelectedBatch = matched;
                    RawMeatQuantityKg = matched.CurrentQuantityKg;
                    await RecalculateSimulationAsync();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Barkod taranırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Curing Chamber IoT Commands ---
    [RelayCommand]
    public void SelectCuringBatch(SucukCuringBatchDto? batch)
    {
        SelectedCuringBatch = batch;
        if (batch != null)
        {
            NewCuringWeightKg = batch.CurrentWeightKg;
            NewCuringPh = batch.CurrentPh;
            NewCuringTemp = batch.ChamberTemperatureCelsius;
            NewCuringHumidity = batch.ChamberHumidityRh;
        }
    }

    [RelayCommand]
    public async Task RecordCuringMeasurementAsync()
    {
        if (SelectedCuringBatch == null)
        {
            MessageBox.Show("Lütfen kurutma odasından bir sucuk partisi seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var req = new RecordCuringMeasurementRequestDto
            {
                BatchId = SelectedCuringBatch.Id,
                MeasuredWeightKg = NewCuringWeightKg,
                MeasuredPh = NewCuringPh,
                ChamberTemp = NewCuringTemp,
                ChamberHumidity = NewCuringHumidity,
                ButcherNotes = CuringMeasurementNotes
            };

            var success = await _reprocessingService.RecordCuringMeasurementAsync(req);
            if (success)
            {
                StatusMessage = $"✓ Kurutma odası ölçümü kaydedildi: {SelectedCuringBatch.CuringLotNumber}";
                var curings = await _reprocessingService.GetCuringBatchesAsync();
                CuringBatches = new ObservableCollection<SucukCuringBatchDto>(curings);
                SelectedCuringBatch = CuringBatches.FirstOrDefault(c => c.Id == req.BatchId);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ölçüm kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Sensory Organoleptic Commands ---
    [RelayCommand]
    public async Task RecordSensoryCheckAsync()
    {
        try
        {
            var req = new RecordSensoryCheckRequestDto
            {
                BatchLotNumber = !string.IsNullOrWhiteSpace(SensoryBatchLot) 
                    ? SensoryBatchLot 
                    : (SelectedBatch?.LotNumber ?? $"LOT-{DateTime.UtcNow:yyyyMMdd}"),
                ProductName = !string.IsNullOrWhiteSpace(SensoryProductName) 
                    ? SensoryProductName 
                    : (SelectedBatch?.ProductName ?? "Dana Kıyma / Kuşbaşı"),
                InspectorName = SensoryInspector,
                SmellScore = SensorySmellScore,
                TextureElasticityScore = SensoryTextureScore,
                ColorAppearanceScore = SensoryColorScore,
                Notes = SensoryNotes
            };

            var res = await _reprocessingService.RecordSensoryCheckAsync(req);
            var checks = await _reprocessingService.GetSensoryChecksAsync();
            SensoryChecks = new ObservableCollection<OrganolepticSensoryCheckDto>(checks);

            if (!res.IsPassed)
            {
                MessageBox.Show(
                    $"DUYUSAL TESTTEN GEÇEMEDİ!\n\n" +
                    $"Ortalama Skor: {res.AverageScore:N1}/5.0 (Minimum 3.0 gereklidir).\n" +
                    $"Koku: {res.SmellScore}/5 | Doku: {res.TextureElasticityScore}/5 | Renk: {res.ColorAppearanceScore}/5\n\n" +
                    $"Karar: {res.Verdict}\n" +
                    $"Bu partinin şarküteri üretimine veya soslanmaya aktarılması YASAKLANDI. Derhal resmi imha protokolü başlatılmalıdır.",
                    "Duyusal Muayene Reddedildi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    $"✓ Organoleptik duyusal muayene başarıyla geçti.\n\n" +
                    $"Ortalama Skor: {res.AverageScore:N1}/5.0\n" +
                    $"Karar: {res.Verdict}\n" +
                    $"Parti dönüştürme ve işleme için uygundur.",
                    "Duyusal Test Onaylandı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Duyusal muayene kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Official Disposal Protocol Commands ---
    [RelayCommand]
    public async Task CreateDisposalRecordAsync()
    {
        try
        {
            var req = new CreateDisposalRecordRequestDto
            {
                BatchLotNumber = SelectedBatch?.LotNumber ?? $"LOT-{DateTime.UtcNow:yyyyMMdd}-IMHA",
                ProductName = SelectedBatch?.ProductName ?? "Reyon İade / Bozuk Et",
                DisposedQuantityKg = DisposalQuantityKg,
                Reason = DisposalReason,
                SecondApprover = DisposalHaccpOfficer,
                FirstApprover = DisposalButcherChief,
                RenderingCompany = DisposalRenderingCompany,
                WaybillNumber = DisposalWaybillNo
            };

            var record = await _reprocessingService.CreateDisposalRecordAsync(req);
            var records = await _reprocessingService.GetDisposalRecordsAsync();
            DisposalRecords = new ObservableCollection<ReprocessingDisposalRecordDto>(records);

            MessageBox.Show(
                $"RESMİ İMHA VE BERTARAF PROTOKOLÜ ONAYLANDI!\n\n" +
                $"Protokol No: {record.ProtocolNumber}\n" +
                $"Parti: {record.BatchLotNumber} ({record.ProductName})\n" +
                $"Miktar: {record.DisposedQuantityKg:N2} kg\n" +
                $"HACCP Onayı: {record.SecondApprover}\n" +
                $"Kasap Şefi Onayı: {record.FirstApprover}\n" +
                $"Lisanslı Bertaraf Firması: {record.RenderingCompanyName}\n" +
                $"İrsaliye: {record.WaybillNumber}\n\n" +
                $"Gıda mevzuatı gereği resmi bertaraf kütüğüne işlenmiştir.",
                "İmha Protokolü Hazırlandı",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"İmha kaydı oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Staff Waste Prevention Incentives Commands ---
    [RelayCommand]
    public async Task RefreshIncentivesAsync()
    {
        var incentives = await _reprocessingService.GetButcherIncentivesAsync();
        ButcherIncentives = new ObservableCollection<ButcherWasteIncentiveDto>(incentives);
    }
}
