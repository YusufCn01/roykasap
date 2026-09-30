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

public partial class ProductionViewModel : ObservableObject
{
    private readonly IProductionService _productionService;
    private readonly IProductService _productService;
    private readonly IMeatProcessingService _meatProcessingService;
    private readonly INavigationService _navService;
    private readonly ShelfLifeReprocessingViewModel _reprocessingVM;

    [ObservableProperty]
    private ObservableCollection<AnimalLotDto> animalLots = new();

    [ObservableProperty]
    private AnimalLotDto? selectedLot;

    [ObservableProperty]
    private ObservableCollection<CuttingTemplateDto> templates = new();

    [ObservableProperty]
    private CuttingTemplateDto? selectedTemplate;

    [ObservableProperty]
    private ObservableCollection<ProductionOutputDto> currentOutputs = new();

    [ObservableProperty]
    private ObservableCollection<ProductionOrderDto> pastOrders = new();

    // Meat Processing Recipes (BOM)
    [ObservableProperty]
    private ObservableCollection<ProcessingRecipe> processingRecipes = new();

    [ObservableProperty]
    private ProcessingRecipe? selectedProcessingRecipe;

    [ObservableProperty]
    private decimal batchTargetWeightKg = 100m;

    [ObservableProperty]
    private bool isProcessingRecipeModalOpen;

    // New Lot Form Fields
    [ObservableProperty]
    private AnimalType newAnimalType = AnimalType.Buyukbas;

    [ObservableProperty]
    private string newEarTagNumber = "TR3400";

    [ObservableProperty]
    private string newSupplier = "Afyon Besi Çiftliği";

    [ObservableProperty]
    private decimal newLiveWeight = 500m;

    [ObservableProperty]
    private decimal newCarcassWeight = 300m;

    [ObservableProperty]
    private decimal newPurchasePrice = 90000m;

    [ObservableProperty]
    private string newOrigin = "Afyonkarahisar";

    // Summary Calculations
    [ObservableProperty]
    private decimal simulatedTotalOutputWeight;

    [ObservableProperty]
    private decimal simulatedTotalWasteWeight;

    [ObservableProperty]
    private decimal simulatedYieldPercentage;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public IEnumerable<AnimalType> AnimalTypes => Enum.GetValues<AnimalType>();

    public ProductionViewModel(
        IProductionService productionService,
        IProductService productService,
        IMeatProcessingService meatProcessingService,
        INavigationService navService,
        ShelfLifeReprocessingViewModel reprocessingVM)
    {
        _productionService = productionService;
        _productService = productService;
        _meatProcessingService = meatProcessingService;
        _navService = navService;
        _reprocessingVM = reprocessingVM;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public void OpenReprocessing()
    {
        _navService.NavigateTo(_reprocessingVM, "Reyon SKT Kurtarma & Reçeteli Dönüşüm");
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var lots = await _productionService.GetAllLotsAsync();
            AnimalLots = new ObservableCollection<AnimalLotDto>(lots);
            if (lots.Any()) SelectedLot = lots.First();

            var tmpls = await _productionService.GetCuttingTemplatesAsync();
            Templates = new ObservableCollection<CuttingTemplateDto>(tmpls);
            if (tmpls.Any()) SelectedTemplate = tmpls.First();

            var orders = await _productionService.GetAllProductionOrdersAsync();
            PastOrders = new ObservableCollection<ProductionOrderDto>(orders);

            ApplyTemplateToSelectedLot();
        }
        catch { }
    }

    [RelayCommand]
    private void SelectLot(AnimalLotDto? lot)
    {
        SelectedLot = lot;
        ApplyTemplateToSelectedLot();
    }

    [RelayCommand]
    private void SelectTemplate(CuttingTemplateDto? template)
    {
        SelectedTemplate = template;
        ApplyTemplateToSelectedLot();
    }

    [RelayCommand]
    public void ApplyTemplateToSelectedLot()
    {
        if (SelectedLot == null || SelectedTemplate == null) return;

        var inputWeight = SelectedLot.CarcassWeightKg;
        var inputCost = SelectedLot.PurchasePriceTotal;

        var outputs = new List<ProductionOutputDto>();
        decimal totalWeightedPoints = 0;

        foreach (var item in SelectedTemplate.Items)
        {
            var weight = Math.Round(inputWeight * (item.ExpectedPercentage / 100m), 2);
            var ratio = item.IsWaste ? 0.0m : (item.IsByproduct ? 0.5m : item.CostWeightRatio);
            totalWeightedPoints += weight * ratio;

            outputs.Add(new ProductionOutputDto
            {
                ProductId = item.TargetProductId,
                CutName = item.CutName,
                WeightKg = weight,
                IsByproduct = item.IsByproduct,
                IsWaste = item.IsWaste,
                YieldPercentage = item.ExpectedPercentage
            });
        }

        // Allocate costs
        if (totalWeightedPoints > 0)
        {
            foreach (var o in outputs)
            {
                if (!o.IsWaste)
                {
                    var item = SelectedTemplate.Items.FirstOrDefault(i => i.CutName == o.CutName);
                    var ratio = item?.CostWeightRatio ?? 1.0m;
                    if (o.IsByproduct) ratio = 0.5m;

                    var itemPoints = o.WeightKg * ratio;
                    var share = itemPoints / totalWeightedPoints;
                    o.TotalCost = Math.Round(inputCost * share, 2);
                    o.CostPerKg = o.WeightKg > 0 ? Math.Round(o.TotalCost / o.WeightKg, 2) : 0;
                }
            }
        }

        CurrentOutputs = new ObservableCollection<ProductionOutputDto>(outputs);
        RecalculateSimulatedYield();
    }

    [RelayCommand]
    public void RecalculateSimulatedYield()
    {
        if (SelectedLot == null) return;

        var inputWeight = SelectedLot.CarcassWeightKg;
        var inputCost = SelectedLot.PurchasePriceTotal;

        SimulatedTotalOutputWeight = CurrentOutputs.Where(o => !o.IsWaste).Sum(o => o.WeightKg);
        SimulatedTotalWasteWeight = CurrentOutputs.Where(o => o.IsWaste).Sum(o => o.WeightKg);
        SimulatedYieldPercentage = inputWeight > 0 ? Math.Round((SimulatedTotalOutputWeight / inputWeight) * 100m, 1) : 0;

        // Recalculate cost distribution with current modified weights
        decimal totalWeightedPoints = 0;
        foreach (var o in CurrentOutputs)
        {
            var ratio = o.IsWaste ? 0.0m : (o.IsByproduct ? 0.5m : 1.0m);
            totalWeightedPoints += o.WeightKg * ratio;
        }

        if (totalWeightedPoints > 0)
        {
            foreach (var o in CurrentOutputs)
            {
                if (o.IsWaste)
                {
                    o.CostPerKg = 0;
                    o.TotalCost = 0;
                }
                else
                {
                    var ratio = o.IsByproduct ? 0.5m : 1.0m;
                    var itemPoints = o.WeightKg * ratio;
                    var share = itemPoints / totalWeightedPoints;
                    o.TotalCost = Math.Round(inputCost * share, 2);
                    o.CostPerKg = o.WeightKg > 0 ? Math.Round(o.TotalCost / o.WeightKg, 2) : 0;
                }
                o.YieldPercentage = inputWeight > 0 ? Math.Round((o.WeightKg / inputWeight) * 100m, 1) : 0;
            }
        }
    }

    [RelayCommand]
    public async Task CreateNewLotAsync()
    {
        if (NewCarcassWeight <= 0 || NewPurchasePrice <= 0)
        {
            MessageBox.Show("Lütfen karkas ağırlığı ve alış fiyatını giriniz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var lot = new AnimalLot
        {
            AnimalType = NewAnimalType,
            CarcassType = AnimalCarcassType.Karkas,
            EarTagNumber = NewEarTagNumber,
            SupplierName = NewSupplier,
            LiveWeightKg = NewLiveWeight,
            CarcassWeightKg = NewCarcassWeight,
            PurchasePriceTotal = NewPurchasePrice,
            Origin = NewOrigin,
            SlaughterDate = DateTime.UtcNow
        };

        var created = await _productionService.CreateAnimalLotAsync(lot);
        AnimalLots.Insert(0, created);
        SelectedLot = created;
        ApplyTemplateToSelectedLot();
        StatusMessage = $"✓ Yeni parti oluşturuldu: {created.LotNumber}";
    }

    [RelayCommand]
    public async Task CompleteProductionOrderAsync()
    {
        if (SelectedLot == null)
        {
            MessageBox.Show("Lütfen parçalanacak karkas partisini seçin.", "Parti Seçilmedi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var order = new ProductionOrder
        {
            LotId = SelectedLot.Id,
            CuttingTemplateId = SelectedTemplate?.Id,
            ResponsiblePerson = "Mustafa Usta (Kasap)",
            OrderDate = DateTime.UtcNow
        };

        var outputs = CurrentOutputs.Select(o => new ProductionOutput
        {
            ProductId = o.ProductId,
            CutName = o.CutName,
            WeightKg = o.WeightKg,
            CostPerKg = o.CostPerKg,
            YieldPercentage = o.YieldPercentage,
            IsWaste = o.IsWaste,
            IsByproduct = o.IsByproduct
        }).ToList();

        try
        {
            var result = await _productionService.CreateProductionOrderAsync(order, outputs);
            PastOrders.Insert(0, result);
            SelectedLot.IsProcessed = true;
            StatusMessage = $"✓ Parçalama Üretim Emri Tamamlandı! ({result.OrderNumber} - Randıman: %{result.TotalYieldPercentage:N1})";
            MessageBox.Show($"Üretim emri başarıyla tamamlandı!\nDepo stokları güncellendi ve etiket partileri üretildi.\n\nGerçekleşen Randıman: %{result.TotalYieldPercentage:N1}", "Üretim Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Üretim kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task OpenProcessingRecipeModalAsync()
    {
        try
        {
            var recipes = await _meatProcessingService.GetRecipesAsync();
            ProcessingRecipes = new ObservableCollection<ProcessingRecipe>(recipes);
            if (recipes.Any()) SelectedProcessingRecipe = recipes.First();
            IsProcessingRecipeModalOpen = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Reçeteler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void CloseProcessingRecipeModal()
    {
        IsProcessingRecipeModalOpen = false;
    }

    [RelayCommand]
    public async Task ExecuteProcessingBatchAsync()
    {
        if (SelectedProcessingRecipe == null)
        {
            MessageBox.Show("Lütfen üretilecek şarküteri reçetesini seçin.", "Reçete Seçilmedi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var prodOrder = await _meatProcessingService.ExecuteProcessingBatchAsync(
                SelectedProcessingRecipe.Id, 
                BatchTargetWeightKg, 
                "Mustafa Usta (Şarküteri Şefi)");

            IsProcessingRecipeModalOpen = false;
            StatusMessage = $"✓ {SelectedProcessingRecipe.RecipeName} Parti Üretimi Başarıyla Tamamlandı! ({BatchTargetWeightKg:N1} kg girdi -> {prodOrder.TotalOutputWeightKg:N1} kg mamul)";
            MessageBox.Show($"Parti üretimi başarıyla tamamlandı!\n\nReçete: {SelectedProcessingRecipe.RecipeName}\nGirdi: {BatchTargetWeightKg:N1} kg\nNet Mamul: {prodOrder.TotalOutputWeightKg:N1} kg\nParti No: {prodOrder.OrderNumber}", "Şarküteri Üretimi Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Parti üretimi sırasında hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
