using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Application.Interfaces.Adapters;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.WPF.ViewModels;

public partial class CustomerAccountsViewModel : ObservableObject
{
    private readonly IFinanceService _financeService;
    private List<CustomerDto> _allCustomers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<CustomerTransaction> transactions = new();

    [ObservableProperty]
    private CariAgingReportDto agingSummary = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string activeFilter = "ALL"; // ALL, OVERDUE, DEBTORS

    // Quick Payment Modal
    [ObservableProperty]
    private bool isPaymentModalOpen;

    [ObservableProperty]
    private decimal paymentAmount;

    [ObservableProperty]
    private string paymentMethodName = "Nakit"; // Nakit, KrediKarti, Havale, Cek

    [ObservableProperty]
    private string paymentDescription = "Cari Hesap Tahsilatı";

    [ObservableProperty]
    private string paymentDocNo = string.Empty;

    // Quick Debit Modal
    [ObservableProperty]
    private bool isDebitModalOpen;

    [ObservableProperty]
    private decimal debitAmount;

    [ObservableProperty]
    private string debitDescription = "Toptan Karkas / Et Sevk Borçlandırması";

    [ObservableProperty]
    private string debitDocNo = string.Empty;

    // New Customer Form
    [ObservableProperty]
    private string newCustomerName = string.Empty;

    [ObservableProperty]
    private string newCustomerPhone = string.Empty;

    [ObservableProperty]
    private string newCustomerTaxNo = string.Empty;

    [ObservableProperty]
    private decimal newCustomerCreditLimit = 25000m;

    [ObservableProperty]
    private int newCustomerTermDays = 30;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public CustomerAccountsViewModel(IFinanceService financeService)
    {
        _financeService = financeService;
        _ = LoadCustomersAsync();
    }

    public async Task LoadCustomersAsync()
    {
        try
        {
            _allCustomers = await _financeService.GetAllCustomersAsync();
            AgingSummary = await _financeService.GetCariAgingReportAsync();
            ApplyFilter();

            if (Customers.Any() && SelectedCustomer == null)
            {
                SelectedCustomer = Customers.First();
                await LoadStatementAsync();
            }
        }
        catch { }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    public void SetFilter(string filter)
    {
        ActiveFilter = filter;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = _allCustomers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(search) || 
                (c.PhoneNumber != null && c.PhoneNumber.Contains(search)) ||
                (c.TaxNumberOrId != null && c.TaxNumberOrId.Contains(search)) ||
                c.Code.ToLower().Contains(search));
        }

        if (ActiveFilter == "OVERDUE")
        {
            query = query.Where(c => c.IsOverdue);
        }
        else if (ActiveFilter == "DEBTORS")
        {
            query = query.Where(c => c.CurrentBalance > 0);
        }

        Customers = new ObservableCollection<CustomerDto>(query.ToList());
    }

    [RelayCommand]
    public async Task SelectCustomerAsync(CustomerDto? cust)
    {
        SelectedCustomer = cust;
        await LoadStatementAsync();
    }

    private async Task LoadStatementAsync()
    {
        if (SelectedCustomer == null) return;
        var st = await _financeService.GetCustomerStatementAsync(SelectedCustomer.Id);
        Transactions = new ObservableCollection<CustomerTransaction>(st);
    }

    [RelayCommand]
    public void OpenPaymentModal()
    {
        if (SelectedCustomer == null)
        {
            MessageBox.Show("Lütfen önce listeden bir cari hesap seçiniz.", "Cari Seçiniz", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        PaymentAmount = Math.Max(0, SelectedCustomer.CurrentBalance);
        PaymentDocNo = $"THS-{DateTime.Now:yyMMdd-HHmm}";
        PaymentDescription = $"{SelectedCustomer.Name} Cari Hesap Tahsilatı";
        IsPaymentModalOpen = true;
    }

    [RelayCommand]
    public void ClosePaymentModal()
    {
        IsPaymentModalOpen = false;
    }

    [RelayCommand]
    public async Task SubmitPaymentAsync()
    {
        if (SelectedCustomer == null || PaymentAmount <= 0)
        {
            MessageBox.Show("Geçerli bir tahsilat tutarı giriniz.", "Geçersiz Tutar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        PaymentType type = PaymentType.Nakit;
        if (PaymentMethodName == "KrediKarti") type = PaymentType.KrediKarti;
        else if (PaymentMethodName == "Havale") type = PaymentType.HavaleEFT;

        var success = await _financeService.RecordPaymentAsync(
            SelectedCustomer.Id, 
            PaymentAmount, 
            type, 
            PaymentDescription, 
            PaymentDocNo);

        if (success)
        {
            IsPaymentModalOpen = false;
            StatusMessage = $"✓ {PaymentAmount:N2} ₺ tahsilat başarıyla kaydedildi.";
            await LoadCustomersAsync();
            await LoadStatementAsync();
            MessageBox.Show($"Tahsilat makbuzu oluşturuldu ve {SelectedCustomer.Name} cari bakiyesinden düşüldü.", "Tahsilat Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    public void OpenDebitModal()
    {
        if (SelectedCustomer == null)
        {
            MessageBox.Show("Lütfen önce listeden bir cari hesap seçiniz.", "Cari Seçiniz", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DebitAmount = 0;
        DebitDocNo = $"BRC-{DateTime.Now:yyMMdd-HHmm}";
        DebitDescription = $"{SelectedCustomer.Name} Toptan Et Sevk İrsaliyesi";
        IsDebitModalOpen = true;
    }

    [RelayCommand]
    public void CloseDebitModal()
    {
        IsDebitModalOpen = false;
    }

    [RelayCommand]
    public async Task SubmitDebitAsync()
    {
        if (SelectedCustomer == null || DebitAmount <= 0)
        {
            MessageBox.Show("Geçerli bir borç tutarı giriniz.", "Geçersiz Tutar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var success = await _financeService.RecordDebitAsync(
            SelectedCustomer.Id, 
            DebitAmount, 
            DebitDescription, 
            DebitDocNo);

        if (success)
        {
            IsDebitModalOpen = false;
            StatusMessage = $"✓ {DebitAmount:N2} ₺ borç dekontu kaydedildi.";
            await LoadCustomersAsync();
            await LoadStatementAsync();
            MessageBox.Show($"Borç dekontu oluşturuldu ve {SelectedCustomer.Name} cari bakiyesine eklendi.", "Borç Dekontu Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    public async Task ExportStatementPdfAsync()
    {
        if (SelectedCustomer == null)
        {
            MessageBox.Show("Lütfen önce bir cari hesap seçiniz.", "Cari Seçiniz", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var htmlBytes = await _financeService.ExportCustomerStatementPdfAsync(SelectedCustomer.Id);
            var tempPath = Path.Combine(Path.GetTempPath(), $"Cari_Ekstre_{SelectedCustomer.Code}_{DateTime.Now:yyyyMMddHHmmss}.html");
            await File.WriteAllBytesAsync(tempPath, htmlBytes);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);

            StatusMessage = $"✓ Cari hesap ekstresi ve mutabakat formu açıldı: {SelectedCustomer.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ekstre yazdırma hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task CreateCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCustomerName))
        {
            MessageBox.Show("Müşteri veya Firma adı zorunludur.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var customer = new Customer
        {
            Name = NewCustomerName.Trim(),
            PhoneNumber = NewCustomerPhone.Trim(),
            TaxNumberOrId = NewCustomerTaxNo.Trim(),
            CreditLimit = NewCustomerCreditLimit,
            PaymentTermDays = NewCustomerTermDays
        };

        var saved = await _financeService.SaveCustomerAsync(customer);
        NewCustomerName = string.Empty;
        NewCustomerPhone = string.Empty;
        NewCustomerTaxNo = string.Empty;

        await LoadCustomersAsync();
        StatusMessage = $"✓ Cari müşteri eklendi: {saved.Name}";
    }

    [RelayCommand]
    public async Task SendOverdueSmsAsync(CustomerDto? cust)
    {
        if (cust == null || string.IsNullOrEmpty(cust.PhoneNumber))
        {
            MessageBox.Show("Müşterinin kayıtlı telefon numarası bulunamadı.", "Telefon Yok", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sent = await _financeService.SendOverduePaymentSmsReminderAsync(cust.Id);
        if (sent)
        {
            StatusMessage = $"✓ SMS Hatırlatma gönderildi: {cust.Name}";
            MessageBox.Show($"{cust.Name} ({cust.PhoneNumber}) müşterisine borç hatırlatma SMS'i iletildi.", "SMS Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public partial class EInvoiceViewModel : ObservableObject
{
    private readonly KasapDbContext _context;
    private readonly IEInvoicerAdapter _eInvoicer;

    [ObservableProperty]
    private ObservableCollection<Invoice> invoices = new();

    [ObservableProperty]
    private Invoice? selectedInvoice;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public EInvoiceViewModel(KasapDbContext context, IEInvoicerAdapter eInvoicer)
    {
        _context = context;
        _eInvoicer = eInvoicer;
        _ = LoadInvoicesAsync();
    }

    public async Task LoadInvoicesAsync()
    {
        try
        {
            var list = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            if (!list.Any())
            {
                // Create a sample draft invoice if empty
                var sample = new Invoice
                {
                    InvoiceNumber = $"GIB2026{DateTime.Now:MMddHHmm}",
                    Uuid = Guid.NewGuid().ToString(),
                    InvoiceType = InvoiceType.SatisFaturasi,
                    EInvoiceStatus = EInvoiceStatus.Taslak,
                    InvoiceDate = DateTime.UtcNow,
                    SubTotal = 3450.00m,
                    VatTotal = 34.50m,
                    GrandTotal = 3484.50m,
                    Notes = "Toptan Et Satışı E-Faturası",
                    Items = new List<InvoiceItem>
                    {
                        new() { ProductName = "Dana Bonfile", Quantity = 3.0m, UnitPrice = 850m, VatRate = 1.0m, VatAmount = 25.50m },
                        new() { ProductName = "Kuzu Pirzola", Quantity = 2.0m, UnitPrice = 450m, VatRate = 1.0m, VatAmount = 9.00m }
                    }
                };
                await _context.Invoices.AddAsync(sample);
                await _context.SaveChangesAsync();
                list.Add(sample);
            }

            Invoices = new ObservableCollection<Invoice>(list);
            SelectedInvoice = list.FirstOrDefault();
        }
        catch { }
    }

    [RelayCommand]
    public async Task SendToGibAsync(Invoice? invoice)
    {
        if (invoice == null) return;

        var result = await _eInvoicer.SendInvoiceAsync(invoice);
        if (result.Success)
        {
            invoice.EInvoiceStatus = EInvoiceStatus.Gonderildi;
            invoice.GibStatusCode = result.GibStatusCode;
            invoice.GibStatusDescription = result.GibStatusMessage;
            await _context.SaveChangesAsync();

            StatusMessage = $"✓ Fatura GİB'e Başarıyla İletildi! (Fatura No: {result.InvoiceNumber})";
            MessageBox.Show($"E-Fatura GİB sistemine başarıyla gönderildi!\nFatura No: {result.InvoiceNumber}\nDurum Kodu: {result.GibStatusCode}", "E-Fatura Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public partial class UserManagementViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<UserDto> users = new();

    [ObservableProperty]
    private UserDto? selectedUser;

    [ObservableProperty]
    private ObservableCollection<Role> roles = new();

    [ObservableProperty]
    private string newUsername = string.Empty;

    [ObservableProperty]
    private string newFullName = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string newPinCode = string.Empty;

    [ObservableProperty]
    private Role? newRole;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public UserManagementViewModel(IAuthService authService)
    {
        _authService = authService;
        _ = LoadUsersAsync();
    }

    public async Task LoadUsersAsync()
    {
        try
        {
            var u = await _authService.GetAllUsersAsync();
            Users = new ObservableCollection<UserDto>(u);
            if (u.Any()) SelectedUser = u.First();

            var r = await _authService.GetAllRolesAsync();
            Roles = new ObservableCollection<Role>(r);
            NewRole = r.FirstOrDefault();
        }
        catch { }
    }

    [RelayCommand]
    public async Task CreateUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword) || NewRole == null)
        {
            MessageBox.Show("Kullanıcı adı, şifre ve rol seçimi zorunludur.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var user = new User
        {
            Username = NewUsername.Trim(),
            FullName = NewFullName.Trim(),
            RoleId = NewRole.Id,
            IsActive = true
        };

        var saved = await _authService.SaveUserAsync(user, NewPassword, NewPinCode);
        if (saved)
        {
            NewUsername = string.Empty;
            NewFullName = string.Empty;
            NewPassword = string.Empty;
            NewPinCode = string.Empty;
            await LoadUsersAsync();
            StatusMessage = "✓ Yeni kullanıcı hesabı başarıyla oluşturuldu.";
            MessageBox.Show("Yeni personel hesabı sisteme kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
