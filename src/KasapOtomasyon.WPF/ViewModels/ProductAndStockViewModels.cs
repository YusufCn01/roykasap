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

public partial class ProductManagementViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IBarcodeService _barcodeService;

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasure> units = new();

    // Editor Fields
    [ObservableProperty]
    private string editCode = string.Empty;

    [ObservableProperty]
    private string editName = string.Empty;

    [ObservableProperty]
    private Category? editCategory;

    [ObservableProperty]
    private UnitOfMeasure? editUnit;

    [ObservableProperty]
    private ProductType editProductType = ProductType.Tartili;

    [ObservableProperty]
    private decimal editSalePrice;

    [ObservableProperty]
    private decimal editCostPrice;

    [ObservableProperty]
    private decimal editVatRate = 1.0m;

    [ObservableProperty]
    private string editBarcode = string.Empty;

    [ObservableProperty]
    private string editPluCode = string.Empty;

    [ObservableProperty]
    private int editShelfLifeDays = 5;

    [ObservableProperty]
    private bool editIsQuickButton = true;

    [ObservableProperty]
    private string generatedPreviewBarcode = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public IEnumerable<ProductType> ProductTypes => Enum.GetValues<ProductType>();

    public ProductManagementViewModel(IProductService productService, IBarcodeService barcodeService)
    {
        _productService = productService;
        _barcodeService = barcodeService;
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var cats = await _productService.GetAllCategoriesAsync();
            Categories = new ObservableCollection<Category>(cats);

            var un = await _productService.GetAllUnitsAsync();
            Units = new ObservableCollection<UnitOfMeasure>(un);

            var prods = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductDto>(prods);
        }
        catch { }
    }

    [RelayCommand]
    private void SelectProduct(ProductDto? product)
    {
        SelectedProduct = product;
        if (product != null)
        {
            EditCode = product.Code;
            EditName = product.Name;
            EditCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
            EditUnit = Units.FirstOrDefault(u => u.Id == product.UnitId);
            EditProductType = product.ProductType;
            EditSalePrice = product.SalePrice;
            EditCostPrice = product.CostPrice;
            EditVatRate = product.VatRate;
            EditBarcode = product.Barcodes.FirstOrDefault() ?? string.Empty;
            EditPluCode = product.PluCode ?? string.Empty;
            EditShelfLifeDays = product.ShelfLifeDays;
            EditIsQuickButton = product.IsQuickButton;

            UpdatePreviewBarcode();
        }
    }

    [RelayCommand]
    private void NewProduct()
    {
        SelectedProduct = null;
        EditCode = $"PRD-{DateTime.Now:MMdd}-{new Random().Next(10, 99)}";
        EditName = string.Empty;
        EditCategory = Categories.FirstOrDefault();
        EditUnit = Units.FirstOrDefault();
        EditProductType = ProductType.Tartili;
        EditSalePrice = 0;
        EditCostPrice = 0;
        EditVatRate = 1.0m;
        EditPluCode = (Products.Count + 1).ToString().PadLeft(5, '0');
        EditBarcode = _barcodeService.GenerateWeightedBarcode(EditPluCode, 1.000m);
        EditShelfLifeDays = 5;
        EditIsQuickButton = true;

        UpdatePreviewBarcode();
    }

    [RelayCommand]
    public void GenerateNewBarcode()
    {
        if (string.IsNullOrEmpty(EditPluCode))
        {
            EditPluCode = new Random().Next(1, 9999).ToString().PadLeft(5, '0');
        }

        if (EditProductType == ProductType.Tartili)
        {
            EditBarcode = _barcodeService.GenerateWeightedBarcode(EditPluCode, 1.000m);
        }
        else
        {
            EditBarcode = _barcodeService.GenerateEan13(EditPluCode);
        }
        UpdatePreviewBarcode();
    }

    private void UpdatePreviewBarcode()
    {
        GeneratedPreviewBarcode = !string.IsNullOrEmpty(EditBarcode) ? EditBarcode : "8690000000000";
    }

    [RelayCommand]
    public async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName) || EditCategory == null || EditUnit == null)
        {
            MessageBox.Show("Ürün adı, kategori ve birim zorunludur!", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var product = new Product
        {
            Id = SelectedProduct?.Id ?? 0,
            Code = EditCode,
            Name = EditName,
            CategoryId = EditCategory.Id,
            UnitOfMeasureId = EditUnit.Id,
            ProductType = EditProductType,
            SalePrice = EditSalePrice,
            CostPrice = EditCostPrice,
            VatRate = EditVatRate,
            PluCode = EditPluCode,
            ShelfLifeDays = EditShelfLifeDays,
            IsQuickButton = EditIsQuickButton,
            DisplayOrder = SelectedProduct?.DisplayOrder ?? Products.Count + 1
        };

        var barcodes = new List<string>();
        if (!string.IsNullOrEmpty(EditBarcode)) barcodes.Add(EditBarcode);

        var saved = await _productService.CreateOrUpdateProductAsync(product, barcodes);
        await LoadDataAsync();
        StatusMessage = $"✓ Ürün başarıyla kaydedildi: {saved.Name}";
    }

    [RelayCommand]
    public async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        if (MessageBox.Show($"'{SelectedProduct.Name}' ürününü silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            await _productService.DeleteProductAsync(SelectedProduct.Id);
            await LoadDataAsync();
            StatusMessage = "Ürün silindi.";
        }
    }
}

public partial class StockViewModel : ObservableObject
{
    private readonly IStockService _stockService;
    private readonly IWmsPalletLogisticsService _palletService;
    private readonly INavigationService _navService;
    private readonly ShelfLifeReprocessingViewModel _reprocessingVM;

    [ObservableProperty]
    private ObservableCollection<StockItemDto> inventory = new();

    [ObservableProperty]
    private ObservableCollection<StockItemDto> criticalAlerts = new();

    [ObservableProperty]
    private ObservableCollection<StockItemDto> expiryAlerts = new();

    [ObservableProperty]
    private ObservableCollection<Warehouse> warehouses = new();

    [ObservableProperty]
    private Warehouse? selectedWarehouse;

    [ObservableProperty]
    private string traceabilitySearch = string.Empty;

    [ObservableProperty]
    private ObservableCollection<StockItemDto> traceabilityResults = new();

    // WMS SSCC-18 Pallets
    [ObservableProperty]
    private ObservableCollection<PalletSSCC> activePallets = new();

    [ObservableProperty]
    private PalletSSCC? selectedPallet;

    [ObservableProperty]
    private bool isPalletBuilderModalOpen;

    [ObservableProperty]
    private string newPalletProductName = "Vakumlu Dana Antrikot (1. Sınıf)";

    [ObservableProperty]
    private string newPalletLotNumber = "LOT-2026-0824-01";

    [ObservableProperty]
    private int newPalletBoxCount = 40;

    [ObservableProperty]
    private decimal newPalletNetWeightKg = 480.50m;

    [ObservableProperty]
    private string newPalletWarehouseBin = "A-02-04-01";

    [ObservableProperty]
    private string newPalletCustomer = "Metro Grossmarket A.Ş.";

    // Transfer fields
    [ObservableProperty]
    private Warehouse? transferSource;

    [ObservableProperty]
    private Warehouse? transferDest;

    [ObservableProperty]
    private StockItemDto? transferProduct;

    [ObservableProperty]
    private decimal transferQuantity = 5.0m;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public StockViewModel(
        IStockService stockService, 
        IWmsPalletLogisticsService palletService,
        INavigationService navService,
        ShelfLifeReprocessingViewModel reprocessingVM)
    {
        _stockService = stockService;
        _palletService = palletService;
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
            var wh = await _stockService.GetAllWarehousesAsync();
            Warehouses = new ObservableCollection<Warehouse>(wh);
            if (wh.Count >= 2)
            {
                TransferSource = wh[0];
                TransferDest = wh[1];
            }

            var inv = await _stockService.GetStockInventoryAsync(SelectedWarehouse?.Id);
            Inventory = new ObservableCollection<StockItemDto>(inv);

            var crits = await _stockService.GetCriticalStockAlertsAsync();
            CriticalAlerts = new ObservableCollection<StockItemDto>(crits);

            var exp = await _stockService.GetCriticalExpiryAlertsAsync();
            ExpiryAlerts = new ObservableCollection<StockItemDto>(exp);

            var pallets = await _palletService.GetActivePalletsAsync();
            ActivePallets = new ObservableCollection<PalletSSCC>(pallets);
        }
        catch { }
    }

    [RelayCommand]
    public void OpenPalletBuilderModal()
    {
        IsPalletBuilderModalOpen = true;
    }

    [RelayCommand]
    public void ClosePalletBuilderModal()
    {
        IsPalletBuilderModalOpen = false;
    }

    [RelayCommand]
    public async Task BuildPalletSSCCAsync()
    {
        try
        {
            var pallet = await _palletService.BuildPalletSSCCAsync(
                NewPalletProductName,
                NewPalletLotNumber,
                NewPalletBoxCount,
                NewPalletNetWeightKg,
                NewPalletWarehouseBin,
                NewPalletCustomer);

            IsPalletBuilderModalOpen = false;
            ActivePallets.Insert(0, pallet);
            SelectedPallet = pallet;
            StatusMessage = $"✓ SSCC-18 Palet Üretildi: {pallet.PalletNumber} ({pallet.Sscc18Barcode})";
            MessageBox.Show($"SSCC-18 Lojistik Palet Başarıyla Oluşturuldu!\n\nPalet No: {pallet.PalletNumber}\nSSCC-18: {pallet.Sscc18Barcode}\nKoli Sayısı: {pallet.TotalBoxesCount}\nNet Ağırlık: {pallet.TotalNetWeightKg:N2} kg\nBrüt Ağırlık: {pallet.TotalGrossWeightKg:N2} kg\nDepo Rafı: {pallet.TargetWarehouseBin}", "Palet Hazır", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Palet oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task QuarantineSelectedPalletAsync(PalletSSCC? pallet)
    {
        if (pallet == null) return;
        var success = await _palletService.QuarantinePalletAsync(pallet.Id, "WMS Kalite / Sıcaklık Sapması Tespiti");
        if (success)
        {
            pallet.IsQuarantined = true;
            StatusMessage = $"🚨 Palet Karantinaya Alındı: {pallet.PalletNumber}";
            MessageBox.Show($"Palet başarıyla karantinaya alındı ve sevkiyata karşı bloke edildi!\n\nPalet No: {pallet.PalletNumber}\nSSCC-18: {pallet.Sscc18Barcode}", "Karantina Uygulandı", MessageBoxButton.OK, MessageBoxImage.Warning);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    public async Task FilterWarehouseAsync(Warehouse? warehouse)
    {
        SelectedWarehouse = warehouse;
        var inv = await _stockService.GetStockInventoryAsync(warehouse?.Id);
        Inventory = new ObservableCollection<StockItemDto>(inv);
    }

    [RelayCommand]
    public async Task SearchTraceabilityAsync()
    {
        if (string.IsNullOrWhiteSpace(TraceabilitySearch)) return;

        var results = await _stockService.GetTraceabilityHistoryAsync(TraceabilitySearch.Trim());
        TraceabilityResults = new ObservableCollection<StockItemDto>(results);
        if (!results.Any())
        {
            MessageBox.Show("Bu parti / barkoda ait et ürünü kaydı bulunamadı.", "Kayıt Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    public async Task ExecuteTransferAsync()
    {
        if (TransferSource == null || TransferDest == null || TransferProduct == null || TransferQuantity <= 0)
        {
            MessageBox.Show("Lütfen kaynak depo, hedef depo, ürün ve geçerli bir transfer miktarı seçin.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var success = await _stockService.TransferStockAsync(TransferSource.Id, TransferDest.Id, TransferProduct.ProductId, TransferQuantity, TransferProduct.LotNumber, "Kullanıcı");
        if (success)
        {
            StatusMessage = $"✓ {TransferQuantity} kg {TransferProduct.ProductName} transfer edildi: {TransferSource.Name} -> {TransferDest.Name}";
            await LoadDataAsync();
        }
        else
        {
            MessageBox.Show("Transfer başarısız! Kaynak depodaki stok miktarı yetersiz olabilir.", "Transfer Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
