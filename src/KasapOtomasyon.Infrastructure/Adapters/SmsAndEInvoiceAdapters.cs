using System.Text;
using KasapOtomasyon.Application.Interfaces.Adapters;
using KasapOtomasyon.Domain.Entities;
using Serilog;

namespace KasapOtomasyon.Infrastructure.Adapters;

public class MockSmsService : ISmsService
{
    public string ProviderName => "MOCK_SMS_PROVIDER";

    public Task<bool> SendSmsAsync(string phoneNumber, string messageContent)
    {
        Log.Information("[SMS GÖNDERİLDİ] Alıcı: {Phone}, Mesaj: {Msg}", phoneNumber, messageContent);
        return Task.FromResult(true);
    }

    public Task<bool> SendOverdueReminderAsync(string customerName, string phoneNumber, decimal overdueAmount, int daysOverdue)
    {
        var message = $"Sayin {customerName}, Kasap Otomasyon uzerindeki {overdueAmount:N2} TL tutarindaki bakiyenizin vadesi {daysOverdue} gun gecmistir. Bilgilerinize sunar, iyi gunler dileriz.";
        return SendSmsAsync(phoneNumber, message);
    }
}

public class NetgsmSmsService : ISmsService
{
    public string ProviderName => "NETGSM";

    public async Task<bool> SendSmsAsync(string phoneNumber, string messageContent)
    {
        try
        {
            // Integration template for Netgsm XML API
            Log.Information("[NETGSM SMS] {Phone} numarasına gönderiliyor: {Msg}", phoneNumber, messageContent);
            await Task.Delay(100); // Simulate API latency
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Netgsm SMS gönderim hatası");
            return false;
        }
    }

    public Task<bool> SendOverdueReminderAsync(string customerName, string phoneNumber, decimal overdueAmount, int daysOverdue)
    {
        var message = $"Sayin {customerName}, {overdueAmount:N2} TL gecikmis cari hesap borcunuz bulunmaktadir. Lutfen kontrol ediniz.";
        return SendSmsAsync(phoneNumber, message);
    }
}

public class MockEInvoicerAdapter : IEInvoicerAdapter
{
    public string IntegratorName => "MOCK_INTEGRATOR";

    public Task<EInvoiceResult> SendInvoiceAsync(Invoice invoice)
    {
        var uuid = string.IsNullOrEmpty(invoice.Uuid) ? Guid.NewGuid().ToString() : invoice.Uuid;
        var invoiceNo = $"GIB2026{new Random().Next(100000000, 999999999)}";

        Log.Information("[E-FATURA GÖNDERİLDİ] No: {No}, UUID: {Uuid}, Tutar: {Total} TL", invoiceNo, uuid, invoice.GrandTotal);

        return Task.FromResult(new EInvoiceResult
        {
            Success = true,
            InvoiceUuid = uuid,
            InvoiceNumber = invoiceNo,
            GibStatusCode = "1200",
            GibStatusMessage = "GİB'e Başarıyla İletildi (Test Modu)",
            SignedXml = $"<Invoice xmlns='urn:oasis:names:specification:ubl:schema:xsd:Invoice-2'><ID>{invoiceNo}</ID><UUID>{uuid}</UUID></Invoice>"
        });
    }

    public Task<EInvoiceResult> CheckInvoiceStatusAsync(string invoiceUuid)
    {
        return Task.FromResult(new EInvoiceResult
        {
            Success = true,
            InvoiceUuid = invoiceUuid,
            GibStatusCode = "1300",
            GibStatusMessage = "GİB Tarafından Başarıyla Onaylandı"
        });
    }

    public Task<byte[]> DownloadInvoicePdfAsync(string invoiceUuid)
    {
        var samplePdfBytes = Encoding.UTF8.GetBytes($"%PDF-1.4 Mock E-Fatura Belgesi UUID: {invoiceUuid}");
        return Task.FromResult(samplePdfBytes);
    }
}
