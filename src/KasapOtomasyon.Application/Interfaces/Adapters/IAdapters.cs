using KasapOtomasyon.Domain.Entities;

namespace KasapOtomasyon.Application.Interfaces.Adapters;

public class ScaleReadingResult
{
    public bool Success { get; set; }
    public decimal WeightKg { get; set; }
    public bool IsStable { get; set; }
    public string Unit { get; set; } = "kg";
    public string ErrorMessage { get; set; } = string.Empty;
}

public interface ITeraziAdapter
{
    string BrandName { get; } // "CAS", "DIGI", "BAYKON", "GENERIC_SERIAL", "SIMULATOR"
    bool IsConnected { get; }
    Task<bool> ConnectAsync(string portName, int baudRate = 9600);
    Task DisconnectAsync();
    Task<ScaleReadingResult> ReadWeightAsync();
    Task<bool> TareAsync();
    Task<bool> ZeroAsync();
}

public interface ISmsService
{
    string ProviderName { get; } // "NETGSM", "ILETI_MERKEZI", "MOCK"
    Task<bool> SendSmsAsync(string phoneNumber, string messageContent);
    Task<bool> SendOverdueReminderAsync(string customerName, string phoneNumber, decimal overdueAmount, int daysOverdue);
}

public class EInvoiceResult
{
    public bool Success { get; set; }
    public string InvoiceUuid { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string GibStatusCode { get; set; } = string.Empty;
    public string GibStatusMessage { get; set; } = string.Empty;
    public string SignedXml { get; set; } = string.Empty;
    public string ErrorDetails { get; set; } = string.Empty;
}

public interface IEInvoicerAdapter
{
    string IntegratorName { get; } // "UYUMSOFT", "LOGO", "FORIBA", "MOCK_INTEGRATOR"
    Task<EInvoiceResult> SendInvoiceAsync(Invoice invoice);
    Task<EInvoiceResult> CheckInvoiceStatusAsync(string invoiceUuid);
    Task<byte[]> DownloadInvoicePdfAsync(string invoiceUuid);
}
