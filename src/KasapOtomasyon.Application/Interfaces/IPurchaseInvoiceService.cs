using KasapOtomasyon.Application.DTOs;

namespace KasapOtomasyon.Application.Interfaces;

public interface IPurchaseInvoiceService
{
    Task<List<PurchaseInvoiceDto>> GetAllPurchaseInvoicesAsync(PurchaseInvoiceFilterDto? filter = null);
    Task<PurchaseInvoiceDto?> GetPurchaseInvoiceByIdAsync(int id);
    Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDto dto);
    Task<bool> CancelPurchaseInvoiceAsync(int id, string reason);
    Task<PurchaseKpiSummaryDto> GetPurchaseKpisAsync();
    Task<byte[]> ExportPurchaseInvoicePdfAsync(int id);
    Task<byte[]> ExportPurchaseInvoicesExcelAsync(PurchaseInvoiceFilterDto? filter = null);
}
