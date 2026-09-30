using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using Microsoft.Win32;

namespace KasapOtomasyon.WPF.ViewModels;

public partial class PurchaseInvoiceViewModel : ObservableObject
{
    private readonly IPurchaseInvoiceService _invoiceService;
    private readonly IFinanceService _financeService;
    private readonly IProductService _productService;

    [ObservableProperty]
    private ObservableCollection<PurchaseInvoiceDto> invoices = new();

    [ObservableProperty]
    private PurchaseInvoiceDto? selectedInvoice;

    [ObservableProperty]
    private PurchaseKpiSummaryDto kpiSummary = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> suppliers = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private string activeTab = "LIST"; // LIST, CREATE

    [ObservableProperty]
    private string statusMessage = string.Empty;

    // Filters
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string filterInvoiceType = "ALL"; // ALL, TICARI, MUSTAHSIL

    // ==========================================
    // NEW INVOICE FORM STATE
    // ==========================================
    [ObservableProperty]
    private string newInvoiceNumber = string.Empty;

    [ObservableProperty]
    private string newWaybillNumber = string.Empty;

    [ObservableProperty]
    private PurchaseInvoiceType newInvoiceType = PurchaseInvoiceType.TicariAlimFaturasi;

    [ObservableProperty]
    private CustomerDto? selectedSupplier;

    [ObservableProperty]
    private DateTime newInvoiceDate = DateTime.Now;

    [ObservableProperty]
    private DateTime? newDueDate = DateTime.Now.AddDays(30);

    [ObservableProperty]
    private string newNotes = string.Empty;

    [ObservableProperty]
    private bool newAutoStock = true;

    [ObservableProperty]
    private ObservableCollection<CreatePurchaseInvoiceLineDto> newInvoiceLines = new();

    // Current Line Item Editor
    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private string lineItemName = "Dana Karkas (Büyükbaş)";

    [ObservableProperty]
    private PurchaseItemType lineItemType = PurchaseItemType.KarkasEt;

    [ObservableProperty]
    private decimal lineQuantity = 350.0m;

    [ObservableProperty]
    private string lineUnit = "Kg";

    [ObservableProperty]
    private decimal lineUnitPrice = 385.0m;

    [ObservableProperty]
    private decimal lineVatRate = 1.0m; // %1 Toptan Et / Canlı Hayvan KDV

    [ObservableProperty]
    private decimal lineDiscountRate = 0m;

    [ObservableProperty]
    private decimal lineTevkifatRate = 0m; // 0.90 for 9/10

    [ObservableProperty]
    private string lineLotNumber = string.Empty;

    [ObservableProperty]
    private string lineEarTagNumber = string.Empty;

    [ObservableProperty]
    private string lineStorageLocation = "Soğuk Depo #1 - Askı Ray-2";

    // Calculated Totals for New Invoice
    [ObservableProperty]
    private decimal liveSubTotal;

    [ObservableProperty]
    private decimal liveDiscountTotal;

    [ObservableProperty]
    private decimal liveVatTotal;

    [ObservableProperty]
    private decimal liveTevkifatTotal;

    [ObservableProperty]
    private decimal liveWithholdingTotal;

    [ObservableProperty]
    private decimal liveSgkTotal;

    [ObservableProperty]
    private decimal liveBorsaTotal;

    [ObservableProperty]
    private decimal liveGrandTotal;

    public PurchaseInvoiceViewModel(
        IPurchaseInvoiceService invoiceService,
        IFinanceService financeService,
        IProductService productService)
    {
        _invoiceService = invoiceService;
        _financeService = financeService;
        _productService = productService;

        ResetNewInvoiceForm();
        _ = LoadInitialDataAsync();
    }

    public async Task LoadInitialDataAsync()
    {
        try
        {
            var custs = await _financeService.GetAllCustomersAsync();
            Suppliers = new ObservableCollection<CustomerDto>(custs);

            var prods = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductDto>(prods);

            await LoadInvoicesAsync();
        }
        catch { }
    }

    public async Task LoadInvoicesAsync()
    {
        try
        {
            var filter = new PurchaseInvoiceFilterDto
            {
                SearchQuery = SearchText
            };

            if (FilterInvoiceType == "TICARI")
                filter.InvoiceType = PurchaseInvoiceType.TicariAlimFaturasi;
            else if (FilterInvoiceType == "MUSTAHSIL")
                filter.InvoiceType = PurchaseInvoiceType.MustahsilMakbuzu;

            var list = await _invoiceService.GetAllPurchaseInvoicesAsync(filter);
            Invoices = new ObservableCollection<PurchaseInvoiceDto>(list);

            KpiSummary = await _invoiceService.GetPurchaseKpisAsync();

            if (Invoices.Any() && SelectedInvoice == null)
            {
                SelectedInvoice = Invoices.First();
            }
        }
        catch { }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadInvoicesAsync();
    }

    [RelayCommand]
    public async Task SetFilterTypeAsync(string filterType)
    {
        FilterInvoiceType = filterType;
        await LoadInvoicesAsync();
    }

    [RelayCommand]
    public void SwitchTab(string tab)
    {
        ActiveTab = tab;
    }

    partial void OnSelectedProductChanged(ProductDto? value)
    {
        if (value != null)
        {
            LineItemName = value.Name;
            LineUnitPrice = value.CostPrice > 0 ? value.CostPrice : (value.SalePrice * 0.75m);
            LineUnit = value.UnitName ?? "Kg";
            LineVatRate = value.VatRate;
        }
    }

    partial void OnNewInvoiceTypeChanged(PurchaseInvoiceType value)
    {
        if (value == PurchaseInvoiceType.MustahsilMakbuzu)
        {
            LineVatRate = 0m;
            LineTevkifatRate = 0m;
        }
        else
        {
            LineVatRate = 1.0m;
        }
        RecalculateTotals();
    }

    [RelayCommand]
    public void AddLine()
    {
        if (LineQuantity <= 0 || LineUnitPrice <= 0)
        {
            MessageBox.Show("Miktar ve Birim Fiyat sıfırdan büyük olmalıdır.", "Geçersiz Kalem", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var line = new CreatePurchaseInvoiceLineDto
        {
            ProductId = SelectedProduct?.Id,
            ItemName = string.IsNullOrWhiteSpace(LineItemName) ? (SelectedProduct?.Name ?? "Et Kalemi") : LineItemName,
            ItemType = LineItemType,
            Quantity = LineQuantity,
            Unit = LineUnit,
            UnitPrice = LineUnitPrice,
            VatRate = NewInvoiceType == PurchaseInvoiceType.MustahsilMakbuzu ? 0m : LineVatRate,
            DiscountRate = LineDiscountRate,
            TevkifatRate = LineTevkifatRate,
            LotNumber = string.IsNullOrWhiteSpace(LineLotNumber) ? $"LOT-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}" : LineLotNumber,
            EarTagNumber = LineEarTagNumber,
            StorageLocation = LineStorageLocation
        };

        NewInvoiceLines.Add(line);
        RecalculateTotals();

        // Prepare for next entry
        LineLotNumber = string.Empty;
        LineEarTagNumber = string.Empty;
    }

    [RelayCommand]
    public void RemoveLine(CreatePurchaseInvoiceLineDto? line)
    {
        if (line != null)
        {
            NewInvoiceLines.Remove(line);
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        decimal subTotal = 0m;
        decimal discountTotal = 0m;
        decimal vatTotal = 0m;
        decimal tevkifatTotal = 0m;

        foreach (var l in NewInvoiceLines)
        {
            var gross = l.Quantity * l.UnitPrice;
            var disc = l.DiscountRate > 0 ? (gross * (l.DiscountRate / 100m)) : 0m;
            var net = gross - disc;
            var vat = net * (l.VatRate / 100m);
            var tevkifat = l.TevkifatRate > 0 ? (vat * l.TevkifatRate) : 0m;

            subTotal += net;
            discountTotal += disc;
            vatTotal += vat;
            tevkifatTotal += tevkifat;
        }

        LiveSubTotal = subTotal;
        LiveDiscountTotal = discountTotal;
        LiveVatTotal = vatTotal;
        LiveTevkifatTotal = tevkifatTotal;

        if (NewInvoiceType == PurchaseInvoiceType.MustahsilMakbuzu)
        {
            LiveWithholdingTotal = subTotal * 0.01m; // %1 Stopaj
            LiveSgkTotal = subTotal * 0.02m;         // %2 SGK
            LiveBorsaTotal = subTotal * 0.002m;      // %0.2 Borsa + Mera
            LiveGrandTotal = subTotal - (LiveWithholdingTotal + LiveSgkTotal + LiveBorsaTotal);
        }
        else
        {
            LiveWithholdingTotal = 0m;
            LiveSgkTotal = 0m;
            LiveBorsaTotal = 0m;
            LiveGrandTotal = subTotal + vatTotal - tevkifatTotal;
        }
    }

    [RelayCommand]
    public async Task SaveInvoiceAsync()
    {
        if (SelectedSupplier == null)
        {
            MessageBox.Show("Lütfen faturanın ait olduğu tedarikçi veya besici cari hesabını seçiniz.", "Tedarikçi Eksik", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!NewInvoiceLines.Any())
        {
            MessageBox.Show("Faturaya en az 1 adet ürün / karkas kalemi eklemelisiniz.", "Kalem Ekleyiniz", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var createDto = new CreatePurchaseInvoiceDto
            {
                InvoiceNumber = NewInvoiceNumber,
                WaybillNumber = NewWaybillNumber,
                InvoiceType = NewInvoiceType,
                SupplierId = SelectedSupplier.Id,
                InvoiceDate = NewInvoiceDate,
                DueDate = NewDueDate,
                Notes = NewNotes,
                AutoUpdateStock = NewAutoStock,
                Lines = NewInvoiceLines.ToList()
            };

            var saved = await _invoiceService.CreatePurchaseInvoiceAsync(createDto);

            StatusMessage = $"✓ {saved.InvoiceNumber} nolu alım faturası kaydedildi ve depoya işlendi.";
            MessageBox.Show($"{saved.InvoiceNumber} numaralı alım belgesi başarıyla kaydedildi.\n\n• Cari Hesap Borçlandırıldı: {saved.GrandTotal:N2} ₺\n• Stok Hareketleri ve Maliyetler Güncellendi.", "Fatura Kaydedildi", MessageBoxButton.OK, MessageBoxImage.Information);

            ResetNewInvoiceForm();
            ActiveTab = "LIST";
            await LoadInvoicesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fatura kaydetme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void ResetNewInvoiceForm()
    {
        NewInvoiceNumber = $"ALM-{DateTime.Now:yyMMdd}-{new Random().Next(1000, 9999)}";
        NewWaybillNumber = $"IRS-{DateTime.Now:yyMMdd}-{new Random().Next(100, 999)}";
        NewInvoiceDate = DateTime.Now;
        NewDueDate = DateTime.Now.AddDays(30);
        NewNotes = string.Empty;
        NewInvoiceLines.Clear();
        LineLotNumber = $"LOT-{DateTime.Now:yyyyMMdd}";
        LineEarTagNumber = string.Empty;
        RecalculateTotals();
    }

    [RelayCommand]
    public async Task ExportPdfAsync(PurchaseInvoiceDto? inv)
    {
        var target = inv ?? SelectedInvoice;
        if (target == null)
        {
            MessageBox.Show("Lütfen önce bir fatura seçiniz.", "Fatura Seçiniz", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var htmlBytes = await _invoiceService.ExportPurchaseInvoicePdfAsync(target.Id);
            var tempPath = Path.Combine(Path.GetTempPath(), $"Alim_Faturasi_{target.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmss}.html");
            await File.WriteAllBytesAsync(tempPath, htmlBytes);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);

            StatusMessage = $"✓ Fatura yazdırıldı / açıldı: {target.InvoiceNumber}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fatura yazdırma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task ExportExcelAsync()
    {
        try
        {
            var bytes = await _invoiceService.ExportPurchaseInvoicesExcelAsync();
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Dosyası (*.xlsx)|*.xlsx",
                FileName = $"RoyPos_Alim_Faturalari_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(sfd.FileName, bytes);
                StatusMessage = $"✓ Excel raporu kaydedildi: {Path.GetFileName(sfd.FileName)}";
                MessageBox.Show("Alım faturaları listesi Excel dosyası olarak dışa aktarıldı!", "Rapor Hazır", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel ihracı hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task CancelInvoiceAsync(PurchaseInvoiceDto? inv)
    {
        var target = inv ?? SelectedInvoice;
        if (target == null) return;

        var result = MessageBox.Show(
            $"{target.InvoiceNumber} numaralı alım faturasını iptal etmek istediğinize emin misiniz?\n\nBu işlem cari bakiyesini düzeltecek ve stok girişlerini geri alacaktır.",
            "Fatura İptal Onayı",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var ok = await _invoiceService.CancelPurchaseInvoiceAsync(target.Id, "Kullanıcı Tarafından İptal Edildi");
            if (ok)
            {
                StatusMessage = $"✓ Fatura iptal edildi: {target.InvoiceNumber}";
                await LoadInvoicesAsync();
            }
        }
    }
}
