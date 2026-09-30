using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Application.Interfaces.Adapters;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.WPF.ViewModels;

public partial class PosViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly ISaleService _saleService;
    private readonly IBarcodeService _barcodeService;
    private readonly ITeraziAdapter _scaleAdapter;
    private readonly IFinanceService _financeService;
    private readonly IAntiFraudEngineService _antiFraudService;

    [ObservableProperty]
    private ObservableCollection<ProductDto> quickProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> filteredProducts = new();

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private ObservableCollection<PosCartItemDto> cartItems = new();

    [ObservableProperty]
    private PosCartItemDto? selectedCartItem;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<ParkedSaleDto> parkedSales = new();

    [ObservableProperty]
    private string searchBarcodeText = string.Empty;

    [ObservableProperty]
    private decimal liveScaleWeight = 0.000m;

    [ObservableProperty]
    private decimal discountTotal = 0m;

    [ObservableProperty]
    private decimal subTotal = 0m;

    [ObservableProperty]
    private decimal grandTotal = 0m;

    [ObservableProperty]
    private decimal tenderAmount = 0m;

    [ObservableProperty]
    private decimal changeAmount = 0m;

    [ObservableProperty]
    private bool isSplitPaymentDialogOpen;

    [ObservableProperty]
    private decimal splitCashAmount;

    [ObservableProperty]
    private decimal splitCardAmount;

    [ObservableProperty]
    private decimal splitAccountAmount;

    [ObservableProperty]
    private bool isParkedSalesDrawerOpen;

    [ObservableProperty]
    private string lastReceiptMessage = string.Empty;

    public PosViewModel(
        IProductService productService,
        ISaleService saleService,
        IBarcodeService barcodeService,
        ITeraziAdapter scaleAdapter,
        IFinanceService financeService,
        IAntiFraudEngineService antiFraudService)
    {
        _productService = productService;
        _saleService = saleService;
        _barcodeService = barcodeService;
        _scaleAdapter = scaleAdapter;
        _financeService = financeService;
        _antiFraudService = antiFraudService;

        _ = LoadInitialDataAsync();
    }

    public async Task LoadInitialDataAsync()
    {
        try
        {
            var cats = await _productService.GetAllCategoriesAsync();
            Categories = new ObservableCollection<Category>(cats);

            var prods = await _productService.GetAllProductsAsync();
            QuickProducts = new ObservableCollection<ProductDto>(prods.Where(p => p.IsQuickButton));
            FilteredProducts = new ObservableCollection<ProductDto>(prods);

            var custs = await _financeService.GetAllCustomersAsync();
            Customers = new ObservableCollection<CustomerDto>(custs);

            await RefreshParkedSalesAsync();
        }
        catch { }
    }

    [RelayCommand]
    private void FilterByCategory(Category? category)
    {
        SelectedCategory = category;
        if (category == null)
        {
            FilteredProducts = new ObservableCollection<ProductDto>(QuickProducts);
        }
        else
        {
            _ = LoadCategoryProductsAsync(category.Id);
        }
    }

    private async Task LoadCategoryProductsAsync(int categoryId)
    {
        var prods = await _productService.GetProductsByCategoryAsync(categoryId);
        FilteredProducts = new ObservableCollection<ProductDto>(prods);
    }

    [RelayCommand]
    public async Task ReadScaleWeightAsync()
    {
        var reading = await _scaleAdapter.ReadWeightAsync();
        if (reading.Success && reading.WeightKg > 0)
        {
            LiveScaleWeight = reading.WeightKg;

            // If an item in the cart is selected, update its weight
            if (SelectedCartItem != null)
            {
                SelectedCartItem.Quantity = reading.WeightKg;
                RecalculateCart();
            }
        }
    }

    [RelayCommand]
    public void AddProductToCart(ProductDto product)
    {
        var weight = (LiveScaleWeight > 0 && product.ProductType == ProductType.Tartili) ? LiveScaleWeight : 1.0m;

        // Reset live scale weight once applied
        LiveScaleWeight = 0;

        var existing = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null && product.ProductType == ProductType.Adetli)
        {
            existing.Quantity += 1;
        }
        else
        {
            var newItem = new PosCartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Barcode = product.Barcodes.FirstOrDefault(),
                PluCode = product.PluCode,
                Quantity = weight,
                UnitName = product.UnitCode,
                UnitPrice = product.SalePrice,
                VatRate = product.VatRate
            };
            CartItems.Add(newItem);
            SelectedCartItem = newItem;
        }

        RecalculateCart();
    }

    [RelayCommand]
    public async Task ProcessBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchBarcodeText)) return;

        var barcode = SearchBarcodeText.Trim();
        SearchBarcodeText = string.Empty;

        var parsed = _barcodeService.ParseBarcode(barcode);
        var product = await _productService.GetProductByBarcodeAsync(barcode);

        if (product != null)
        {
            var quantity = parsed.IsWeightEmbedded ? parsed.WeightKg : 1.0m;
            var newItem = new PosCartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Barcode = barcode,
                PluCode = product.PluCode,
                Quantity = quantity,
                UnitName = product.UnitCode,
                UnitPrice = product.SalePrice,
                VatRate = product.VatRate
            };
            CartItems.Add(newItem);
            SelectedCartItem = newItem;
            RecalculateCart();
        }
        else
        {
            MessageBox.Show($"'{barcode}' barkoduna ait ürün bulunamadı!", "Barkod Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void RemoveCartItem(PosCartItemDto? item)
    {
        if (item != null && CartItems.Contains(item))
        {
            CartItems.Remove(item);
            RecalculateCart();
        }
    }

    [RelayCommand]
    private void IncreaseQuantity(PosCartItemDto? item)
    {
        if (item != null)
        {
            item.Quantity += (item.UnitName == "KG" || item.UnitName == "GR") ? 0.250m : 1.0m;
            RecalculateCart();
        }
    }

    [RelayCommand]
    private void DecreaseQuantity(PosCartItemDto? item)
    {
        if (item != null && item.Quantity > 0.1m)
        {
            var step = (item.UnitName == "KG" || item.UnitName == "GR") ? 0.250m : 1.0m;
            item.Quantity = Math.Max(0.1m, item.Quantity - step);
            RecalculateCart();
        }
    }

    [RelayCommand]
    private void ApplyDiscount(string percentOrAmount)
    {
        if (decimal.TryParse(percentOrAmount, out var val))
        {
            if (val <= 100)
            {
                // Percentage discount on grand total
                DiscountTotal = Math.Round(SubTotal * (val / 100m), 2);
            }
            else
            {
                // Absolute amount
                DiscountTotal = val;
            }
            RecalculateCart();
        }
    }

    [RelayCommand]
    public void ClearCart()
    {
        CartItems.Clear();
        SelectedCartItem = null;
        DiscountTotal = 0;
        TenderAmount = 0;
        ChangeAmount = 0;
        RecalculateCart();
    }

    private void RecalculateCart()
    {
        SubTotal = CartItems.Sum(i => i.TotalPrice);
        GrandTotal = Math.Max(0, SubTotal - DiscountTotal);
        ChangeAmount = Math.Max(0, TenderAmount - GrandTotal);

        // Reset split dialog defaults
        SplitCashAmount = GrandTotal;
        SplitCardAmount = 0;
        SplitAccountAmount = 0;
    }

    [RelayCommand]
    public async Task CompleteCashSaleAsync()
    {
        await CompleteSaleWithPaymentAsync(PaymentType.Nakit, GrandTotal);
    }

    [RelayCommand]
    public async Task CompleteCardSaleAsync()
    {
        await CompleteSaleWithPaymentAsync(PaymentType.KrediKarti, GrandTotal);
    }

    [RelayCommand]
    public async Task CompleteAccountSaleAsync()
    {
        if (SelectedCustomer == null)
        {
            MessageBox.Show("Açık hesap / Veresiye satışı için lütfen bir cari müşteri seçin!", "Müşteri Seçilmedi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await CompleteSaleWithPaymentAsync(PaymentType.CariHesap, GrandTotal);
    }

    private async Task CompleteSaleWithPaymentAsync(PaymentType paymentType, decimal amount)
    {
        if (!CartItems.Any()) return;

        var request = new CompleteSaleRequestDto
        {
            Items = CartItems.ToList(),
            CustomerId = SelectedCustomer?.Id,
            DiscountTotal = DiscountTotal,
            Payments = new List<SplitPaymentDto>
            {
                new() { PaymentType = paymentType, Amount = amount }
            }
        };

        try
        {
            var sale = await _saleService.CompleteSaleAsync(request);
            LastReceiptMessage = $"✓ Satış Tamamlandı! Fiş No: {sale.ReceiptNumber} | Tutar: {sale.GrandTotal:N2} ₺";
            ClearCart();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Satış kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenSplitPaymentDialog()
    {
        if (!CartItems.Any()) return;
        RecalculateCart();
        IsSplitPaymentDialogOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmSplitPaymentAsync()
    {
        var totalPaid = SplitCashAmount + SplitCardAmount + SplitAccountAmount;
        if (totalPaid < GrandTotal)
        {
            MessageBox.Show($"Ödenen toplam tutar ({totalPaid:N2} ₺) fiş toplamından ({GrandTotal:N2} ₺) az olamaz!", "Eksik Ödeme", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var payments = new List<SplitPaymentDto>();
        if (SplitCashAmount > 0) payments.Add(new SplitPaymentDto { PaymentType = PaymentType.Nakit, Amount = SplitCashAmount });
        if (SplitCardAmount > 0) payments.Add(new SplitPaymentDto { PaymentType = PaymentType.KrediKarti, Amount = SplitCardAmount });
        if (SplitAccountAmount > 0)
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Veresiye ödeme için müşteri seçilmelidir.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            payments.Add(new SplitPaymentDto { PaymentType = PaymentType.CariHesap, Amount = SplitAccountAmount });
        }

        var request = new CompleteSaleRequestDto
        {
            Items = CartItems.ToList(),
            CustomerId = SelectedCustomer?.Id,
            DiscountTotal = DiscountTotal,
            Payments = payments
        };

        var sale = await _saleService.CompleteSaleAsync(request);
        LastReceiptMessage = $"✓ Parçalı Satış Başarılı! Fiş No: {sale.ReceiptNumber} | Tutar: {sale.GrandTotal:N2} ₺";
        IsSplitPaymentDialogOpen = false;
        ClearCart();
    }

    [RelayCommand]
    private async Task ParkCurrentSaleAsync()
    {
        if (!CartItems.Any()) return;

        var request = new CompleteSaleRequestDto
        {
            Items = CartItems.ToList(),
            CustomerId = SelectedCustomer?.Id
        };

        var parked = await _saleService.ParkSaleAsync(request, $"Müşteri: {SelectedCustomer?.Name ?? "Perakende"} ({DateTime.Now:HH:mm})");
        await RefreshParkedSalesAsync();
        ClearCart();
        LastReceiptMessage = $"Satış Beklemeye Alındı. (Toplam: {parked.GrandTotal:N2} ₺)";
    }

    [RelayCommand]
    private async Task RecallParkedSaleAsync(ParkedSaleDto? parked)
    {
        if (parked == null) return;

        CartItems = new ObservableCollection<PosCartItemDto>(parked.Items);
        RecalculateCart();

        await _saleService.RemoveParkedSaleAsync(parked.ParkId);
        await RefreshParkedSalesAsync();
        IsParkedSalesDrawerOpen = false;
    }

    private async Task RefreshParkedSalesAsync()
    {
        var list = await _saleService.GetParkedSalesAsync();
        ParkedSales = new ObservableCollection<ParkedSaleDto>(list);
    }
}
