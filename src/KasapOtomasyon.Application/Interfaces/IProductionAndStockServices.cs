using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.Interfaces;

public interface IProductionService
{
    Task<List<AnimalLotDto>> GetAllLotsAsync();
    Task<AnimalLotDto> CreateAnimalLotAsync(AnimalLot lot);
    Task<List<CuttingTemplateDto>> GetCuttingTemplatesAsync(AnimalType? animalType = null);
    Task<CuttingTemplateDto> SaveCuttingTemplateAsync(CuttingTemplate template);
    Task<ProductionOrderDto> CalculateProductionYieldAsync(int lotId, int? templateId, List<ProductionOutputDto> outputs);
    Task<ProductionOrderDto> CreateProductionOrderAsync(ProductionOrder order, List<ProductionOutput> outputs);
    Task<List<ProductionOrderDto>> GetAllProductionOrdersAsync();
    Task<CuttingYieldReportDto> GetLotYieldReportAsync(int lotId);
}

public interface IStockService
{
    Task<List<StockItemDto>> GetStockInventoryAsync(int? warehouseId = null);
    Task<List<StockItemDto>> GetCriticalStockAlertsAsync();
    Task<List<StockItemDto>> GetCriticalExpiryAlertsAsync();
    Task<List<Warehouse>> GetAllWarehousesAsync();
    Task<bool> RecordStockMovementAsync(StockMovement movement);
    Task<bool> TransferStockAsync(int sourceWarehouseId, int destWarehouseId, int productId, decimal quantity, string? lotNo, string user);
    Task<StockCount> CompleteStockCountAsync(int warehouseId, StockCountMode mode, List<StockCountItem> items, string user);
    Task<List<StockItemDto>> GetTraceabilityHistoryAsync(string lotOrBatchBarcode);
}
