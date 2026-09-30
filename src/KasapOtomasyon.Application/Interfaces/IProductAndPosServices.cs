using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Entities;

namespace KasapOtomasyon.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<List<ProductDto>> GetQuickButtonProductsAsync();
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto?> GetProductByBarcodeAsync(string barcode);
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<UnitOfMeasure>> GetAllUnitsAsync();
    Task<ProductDto> CreateOrUpdateProductAsync(Product product, List<string> barcodes);
    Task<bool> DeleteProductAsync(int id);
}

public interface IBarcodeService
{
    EmbeddedWeightBarcodeDto ParseBarcode(string barcode);
    string GenerateEan13(string productCodeOrPlu);
    string GenerateWeightedBarcode(string pluCode, decimal weightKg);
    string GeneratePriceEmbeddedBarcode(string pluCode, decimal priceTotal);
}

public interface ISaleService
{
    Task<Sale> CompleteSaleAsync(CompleteSaleRequestDto request);
    Task<ParkedSaleDto> ParkSaleAsync(CompleteSaleRequestDto request, string? note);
    Task<List<ParkedSaleDto>> GetParkedSalesAsync();
    Task<bool> RemoveParkedSaleAsync(string parkId);
    Task<Sale?> GetSaleByReceiptNumberAsync(string receiptNumber);
    Task<bool> RefundSaleAsync(int saleId, string reason, int userId);
}
