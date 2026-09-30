using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Services;

public class ProductionService : IProductionService
{
    private readonly KasapDbContext _context;

    public ProductionService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnimalLotDto>> GetAllLotsAsync()
    {
        return await _context.AnimalLots
            .OrderByDescending(l => l.SlaughterDate)
            .Select(l => new AnimalLotDto
            {
                Id = l.Id,
                LotNumber = l.LotNumber,
                AnimalType = l.AnimalType,
                CarcassType = l.CarcassType,
                SupplierName = l.SupplierName,
                EarTagNumber = l.EarTagNumber,
                LiveWeightKg = l.LiveWeightKg,
                CarcassWeightKg = l.CarcassWeightKg,
                PurchasePriceTotal = l.PurchasePriceTotal,
                UnitCostPerKg = l.UnitCostPerKg,
                Origin = l.Origin,
                SlaughterDate = l.SlaughterDate,
                VeterinaryReportNo = l.VeterinaryReportNo,
                TraceabilityCode = l.TraceabilityCode,
                IsProcessed = l.IsProcessed
            })
            .ToListAsync();
    }

    public async Task<AnimalLotDto> CreateAnimalLotAsync(AnimalLot lot)
    {
        if (string.IsNullOrEmpty(lot.LotNumber))
        {
            lot.LotNumber = $"LOT-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}";
        }
        if (string.IsNullOrEmpty(lot.TraceabilityCode))
        {
            lot.TraceabilityCode = $"TR-{lot.EarTagNumber ?? "KASAP"}-{DateTime.UtcNow:yyMMdd}";
        }

        await _context.AnimalLots.AddAsync(lot);
        await _context.SaveChangesAsync();

        return new AnimalLotDto
        {
            Id = lot.Id,
            LotNumber = lot.LotNumber,
            AnimalType = lot.AnimalType,
            CarcassType = lot.CarcassType,
            SupplierName = lot.SupplierName,
            EarTagNumber = lot.EarTagNumber,
            LiveWeightKg = lot.LiveWeightKg,
            CarcassWeightKg = lot.CarcassWeightKg,
            PurchasePriceTotal = lot.PurchasePriceTotal,
            UnitCostPerKg = lot.UnitCostPerKg,
            Origin = lot.Origin,
            SlaughterDate = lot.SlaughterDate,
            VeterinaryReportNo = lot.VeterinaryReportNo,
            TraceabilityCode = lot.TraceabilityCode,
            IsProcessed = lot.IsProcessed
        };
    }

    public async Task<List<CuttingTemplateDto>> GetCuttingTemplatesAsync(AnimalType? animalType = null)
    {
        var query = _context.CuttingTemplates
            .Include(t => t.Items)
            .ThenInclude(i => i.TargetProduct)
            .Where(t => t.IsActive);

        if (animalType.HasValue)
        {
            query = query.Where(t => t.AnimalType == animalType.Value);
        }

        return await query.Select(t => new CuttingTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            AnimalType = t.AnimalType,
            Description = t.Description,
            Items = t.Items.Select(i => new CuttingTemplateItemDto
            {
                Id = i.Id,
                CutName = i.CutName,
                TargetProductId = i.TargetProductId,
                TargetProductName = i.TargetProduct != null ? i.TargetProduct.Name : null,
                ExpectedPercentage = i.ExpectedPercentage,
                CostWeightRatio = i.CostWeightRatio,
                IsByproduct = i.IsByproduct,
                IsWaste = i.IsWaste
            }).ToList()
        }).ToListAsync();
    }

    public async Task<CuttingTemplateDto> SaveCuttingTemplateAsync(CuttingTemplate template)
    {
        if (template.Id == 0)
        {
            await _context.CuttingTemplates.AddAsync(template);
        }
        else
        {
            _context.CuttingTemplates.Update(template);
        }
        await _context.SaveChangesAsync();

        return (await GetCuttingTemplatesAsync()).First(t => t.Id == template.Id);
    }

    public async Task<ProductionOrderDto> CalculateProductionYieldAsync(int lotId, int? templateId, List<ProductionOutputDto> outputs)
    {
        var lot = await _context.AnimalLots.FindAsync(lotId);
        if (lot == null) throw new ArgumentException("Parti bulunamadı.");

        var inputWeight = lot.CarcassWeightKg;
        var inputCost = lot.PurchasePriceTotal;

        var totalOutputWeight = outputs.Where(o => !o.IsWaste).Sum(o => o.WeightKg);
        var totalWasteWeight = outputs.Where(o => o.IsWaste).Sum(o => o.WeightKg);

        // Weighted Cost Allocation across cuts based on weight and cost multiplier
        // Total Weighted Points = SUM(Weight_i * CostWeightRatio_i)
        decimal totalWeightedPoints = 0;
        foreach (var o in outputs)
        {
            var ratio = o.IsWaste ? 0.0m : (o.IsByproduct ? 0.5m : 1.0m);
            totalWeightedPoints += o.WeightKg * ratio;
        }

        if (totalWeightedPoints > 0)
        {
            foreach (var o in outputs)
            {
                if (o.IsWaste)
                {
                    o.CostPerKg = 0;
                    o.TotalCost = 0;
                }
                else
                {
                    var ratio = o.IsByproduct ? 0.5m : 1.0m;
                    var itemWeightedPoints = o.WeightKg * ratio;
                    var itemShare = itemWeightedPoints / totalWeightedPoints;
                    o.TotalCost = Math.Round(inputCost * itemShare, 2);
                    o.CostPerKg = o.WeightKg > 0 ? Math.Round(o.TotalCost / o.WeightKg, 2) : 0;
                }
                o.YieldPercentage = inputWeight > 0 ? Math.Round((o.WeightKg / inputWeight) * 100m, 2) : 0;
            }
        }

        var totalYieldPercent = inputWeight > 0 ? Math.Round((totalOutputWeight / inputWeight) * 100m, 2) : 0;

        return new ProductionOrderDto
        {
            LotId = lot.Id,
            LotNumber = lot.LotNumber,
            AnimalTypeStr = lot.AnimalType.ToString(),
            CuttingTemplateId = templateId,
            OrderDate = DateTime.UtcNow,
            InputWeightKg = inputWeight,
            InputCostTotal = inputCost,
            TotalOutputWeightKg = totalOutputWeight,
            TotalWasteWeightKg = totalWasteWeight,
            TotalYieldPercentage = totalYieldPercent,
            Status = ProductionOrderStatus.Planlandi,
            Outputs = outputs
        };
    }

    public async Task<ProductionOrderDto> CreateProductionOrderAsync(ProductionOrder order, List<ProductionOutput> outputs)
    {
        var lot = await _context.AnimalLots.FindAsync(order.LotId);
        if (lot == null) throw new ArgumentException("Parti bulunamadı.");

        if (string.IsNullOrEmpty(order.OrderNumber))
        {
            order.OrderNumber = $"URT-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}";
        }

        order.OrderDate = DateTime.UtcNow;
        order.Status = ProductionOrderStatus.Tamamlandi;
        order.InputWeightKg = lot.CarcassWeightKg;
        order.InputCostTotal = lot.PurchasePriceTotal;
        order.TotalOutputWeightKg = outputs.Where(o => !o.IsWaste).Sum(o => o.WeightKg);
        order.TotalWasteWeightKg = outputs.Where(o => o.IsWaste).Sum(o => o.WeightKg);
        order.TotalOutputCostTotal = outputs.Sum(o => o.TotalCost);

        var defaultWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.IsDefault) ?? await _context.Warehouses.FirstAsync();

        foreach (var output in outputs)
        {
            output.BatchBarcode = $"BATCH-{order.OrderNumber}-{output.CutName.Replace(" ", "")}";
            output.ExpiryDate = DateTime.UtcNow.AddDays(7);
            order.Outputs.Add(output);

            // If matched to a product, increase inventory
            if (output.ProductId.HasValue && output.WeightKg > 0 && !output.IsWaste)
            {
                var stock = await _context.StockItems.FirstOrDefaultAsync(s => s.WarehouseId == defaultWarehouse.Id && s.ProductId == output.ProductId.Value);
                if (stock != null)
                {
                    stock.CurrentQuantity += output.WeightKg;
                    stock.LotNumber = lot.LotNumber;
                    stock.ExpiryDate = output.ExpiryDate;
                }
                else
                {
                    await _context.StockItems.AddAsync(new StockItem
                    {
                        WarehouseId = defaultWarehouse.Id,
                        ProductId = output.ProductId.Value,
                        CurrentQuantity = output.WeightKg,
                        LotNumber = lot.LotNumber,
                        ExpiryDate = output.ExpiryDate
                    });
                }

                await _context.StockMovements.AddAsync(new StockMovement
                {
                    WarehouseId = defaultWarehouse.Id,
                    ProductId = output.ProductId.Value,
                    MovementType = StockMovementType.UretimGiris,
                    Quantity = output.WeightKg,
                    UnitPrice = output.CostPerKg,
                    ReferenceNumber = order.OrderNumber,
                    LotNumber = lot.LotNumber,
                    Description = $"Mezbaha Parçalama Üretimi: {output.CutName} (Parti: {lot.LotNumber})",
                    MovementDate = DateTime.UtcNow
                });
            }
        }

        lot.IsProcessed = true;

        await _context.ProductionOrders.AddAsync(order);
        await _context.SaveChangesAsync();

        return new ProductionOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            LotId = lot.Id,
            LotNumber = lot.LotNumber,
            OrderDate = order.OrderDate,
            ResponsiblePerson = order.ResponsiblePerson,
            InputWeightKg = order.InputWeightKg,
            InputCostTotal = order.InputCostTotal,
            TotalOutputWeightKg = order.TotalOutputWeightKg,
            TotalYieldPercentage = order.TotalYieldPercentage,
            TotalWasteWeightKg = order.TotalWasteWeightKg,
            Status = order.Status,
            Outputs = outputs.Select(o => new ProductionOutputDto
            {
                ProductId = o.ProductId,
                CutName = o.CutName,
                WeightKg = o.WeightKg,
                CostPerKg = o.CostPerKg,
                TotalCost = o.TotalCost,
                YieldPercentage = o.YieldPercentage,
                BatchBarcode = o.BatchBarcode,
                ExpiryDate = o.ExpiryDate,
                IsWaste = o.IsWaste,
                IsByproduct = o.IsByproduct
            }).ToList()
        };
    }

    public async Task<List<ProductionOrderDto>> GetAllProductionOrdersAsync()
    {
        return await _context.ProductionOrders
            .Include(o => o.AnimalLot)
            .Include(o => o.CuttingTemplate)
            .Include(o => o.Outputs)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new ProductionOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                LotId = o.LotId,
                LotNumber = o.AnimalLot.LotNumber,
                AnimalTypeStr = o.AnimalLot.AnimalType.ToString(),
                CuttingTemplateId = o.CuttingTemplateId,
                CuttingTemplateName = o.CuttingTemplate != null ? o.CuttingTemplate.Name : "Serbest Parçalama",
                OrderDate = o.OrderDate,
                ResponsiblePerson = o.ResponsiblePerson,
                InputWeightKg = o.InputWeightKg,
                InputCostTotal = o.InputCostTotal,
                TotalOutputWeightKg = o.TotalOutputWeightKg,
                TotalYieldPercentage = o.TotalYieldPercentage,
                TotalWasteWeightKg = o.TotalWasteWeightKg,
                Status = o.Status,
                Outputs = o.Outputs.Select(outp => new ProductionOutputDto
                {
                    ProductId = outp.ProductId,
                    CutName = outp.CutName,
                    WeightKg = outp.WeightKg,
                    CostPerKg = outp.CostPerKg,
                    TotalCost = outp.TotalCost,
                    YieldPercentage = outp.YieldPercentage,
                    BatchBarcode = outp.BatchBarcode,
                    ExpiryDate = outp.ExpiryDate,
                    IsWaste = outp.IsWaste,
                    IsByproduct = outp.IsByproduct
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<CuttingYieldReportDto> GetLotYieldReportAsync(int lotId)
    {
        var lot = await _context.AnimalLots.FindAsync(lotId);
        if (lot == null) throw new ArgumentException("Parti bulunamadı.");

        var orders = await _context.ProductionOrders
            .Include(o => o.Outputs)
            .Where(o => o.LotId == lotId)
            .ToListAsync();

        var allOutputs = orders.SelectMany(o => o.Outputs).ToList();

        var edibleWeight = allOutputs.Where(o => !o.IsWaste && !o.IsByproduct).Sum(o => o.WeightKg);
        var byproductWeight = allOutputs.Where(o => o.IsByproduct).Sum(o => o.WeightKg);
        var wasteWeight = allOutputs.Where(o => o.IsWaste).Sum(o => o.WeightKg);

        return new CuttingYieldReportDto
        {
            LotNumber = lot.LotNumber,
            AnimalInfo = $"{lot.AnimalType} - Küpe No: {lot.EarTagNumber ?? "-"}, Menşei: {lot.Origin ?? "-"}",
            InputCarcassWeightKg = lot.CarcassWeightKg,
            InputTotalCost = lot.PurchasePriceTotal,
            TotalEdibleWeightKg = edibleWeight,
            TotalByproductWeightKg = byproductWeight,
            TotalWasteWeightKg = wasteWeight,
            NetMeatYieldPercentage = lot.CarcassWeightKg > 0 ? Math.Round((edibleWeight / lot.CarcassWeightKg) * 100m, 2) : 0,
            WastePercentage = lot.CarcassWeightKg > 0 ? Math.Round((wasteWeight / lot.CarcassWeightKg) * 100m, 2) : 0,
            CutsBreakdown = allOutputs.Select(o => new ProductionOutputDto
            {
                CutName = o.CutName,
                WeightKg = o.WeightKg,
                CostPerKg = o.CostPerKg,
                TotalCost = o.TotalCost,
                YieldPercentage = o.YieldPercentage,
                IsWaste = o.IsWaste,
                IsByproduct = o.IsByproduct
            }).ToList()
        };
    }
}

public class StockService : IStockService
{
    private readonly KasapDbContext _context;

    public StockService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockItemDto>> GetStockInventoryAsync(int? warehouseId = null)
    {
        var query = _context.StockItems
            .Include(s => s.Product)
            .ThenInclude(p => p.Category)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(s => s.WarehouseId == warehouseId.Value);
        }

        return await query.Select(s => new StockItemDto
        {
            ProductId = s.ProductId,
            ProductCode = s.Product.Code,
            ProductName = s.Product.Name,
            CategoryName = s.Product.Category.Name,
            WarehouseId = s.WarehouseId,
            WarehouseName = s.Warehouse.Name,
            CurrentQuantity = s.CurrentQuantity,
            MinStockLevel = s.Product.MinStockLevel,
            MaxStockLevel = s.Product.MaxStockLevel,
            LotNumber = s.LotNumber,
            ExpiryDate = s.ExpiryDate
        }).ToListAsync();
    }

    public async Task<List<StockItemDto>> GetCriticalStockAlertsAsync()
    {
        var all = await GetStockInventoryAsync();
        return all.Where(s => s.IsCriticalStock).ToList();
    }

    public async Task<List<StockItemDto>> GetCriticalExpiryAlertsAsync()
    {
        var all = await GetStockInventoryAsync();
        return all.Where(s => s.IsCriticalExpiry).OrderBy(s => s.ExpiryDate).ToList();
    }

    public async Task<List<Warehouse>> GetAllWarehousesAsync()
    {
        return await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
    }

    public async Task<bool> RecordStockMovementAsync(StockMovement movement)
    {
        await _context.StockMovements.AddAsync(movement);

        var stock = await _context.StockItems.FirstOrDefaultAsync(s => s.WarehouseId == movement.WarehouseId && s.ProductId == movement.ProductId);
        if (stock != null)
        {
            stock.CurrentQuantity += movement.Quantity;
        }
        else
        {
            await _context.StockItems.AddAsync(new StockItem
            {
                WarehouseId = movement.WarehouseId,
                ProductId = movement.ProductId,
                CurrentQuantity = movement.Quantity,
                LotNumber = movement.LotNumber,
                ExpiryDate = movement.ExpiryDate
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TransferStockAsync(int sourceWarehouseId, int destWarehouseId, int productId, decimal quantity, string? lotNo, string user)
    {
        if (sourceWarehouseId == destWarehouseId || quantity <= 0) return false;

        var sourceStock = await _context.StockItems.FirstOrDefaultAsync(s => s.WarehouseId == sourceWarehouseId && s.ProductId == productId);
        if (sourceStock == null || sourceStock.CurrentQuantity < quantity) return false;

        sourceStock.CurrentQuantity -= quantity;

        var destStock = await _context.StockItems.FirstOrDefaultAsync(s => s.WarehouseId == destWarehouseId && s.ProductId == productId);
        if (destStock != null)
        {
            destStock.CurrentQuantity += quantity;
        }
        else
        {
            await _context.StockItems.AddAsync(new StockItem
            {
                WarehouseId = destWarehouseId,
                ProductId = productId,
                CurrentQuantity = quantity,
                LotNumber = lotNo ?? sourceStock.LotNumber,
                ExpiryDate = sourceStock.ExpiryDate
            });
        }

        var transferNo = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}";

        await _context.WarehouseTransfers.AddAsync(new WarehouseTransfer
        {
            TransferNumber = transferNo,
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destWarehouseId,
            TransferDate = DateTime.UtcNow,
            RequestedBy = user,
            ApprovedBy = user,
            Status = "Onaylandi",
            Items = new List<WarehouseTransferItem>
            {
                new() { ProductId = productId, Quantity = quantity, LotNumber = lotNo }
            }
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<StockCount> CompleteStockCountAsync(int warehouseId, StockCountMode mode, List<StockCountItem> items, string user)
    {
        var count = new StockCount
        {
            CountNumber = $"SYM-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}",
            WarehouseId = warehouseId,
            CountDate = DateTime.UtcNow,
            CountMode = mode,
            Status = "Tamamlandi",
            ConductedBy = user,
            Items = items
        };

        // Adjust stock balances to counted quantity
        foreach (var item in items)
        {
            var stock = await _context.StockItems.FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == item.ProductId);
            if (stock != null)
            {
                stock.CurrentQuantity = item.CountedQuantity;
            }
            else
            {
                await _context.StockItems.AddAsync(new StockItem
                {
                    WarehouseId = warehouseId,
                    ProductId = item.ProductId,
                    CurrentQuantity = item.CountedQuantity
                });
            }

            if (item.DifferenceQuantity != 0)
            {
                await _context.StockMovements.AddAsync(new StockMovement
                {
                    WarehouseId = warehouseId,
                    ProductId = item.ProductId,
                    MovementType = StockMovementType.SayimFarki,
                    Quantity = item.DifferenceQuantity,
                    UnitPrice = item.UnitCost,
                    ReferenceNumber = count.CountNumber,
                    Description = $"Sayım Fark Düzeltmesi ({mode})",
                    MovementDate = DateTime.UtcNow
                });
            }
        }

        await _context.StockCounts.AddAsync(count);
        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<List<StockItemDto>> GetTraceabilityHistoryAsync(string lotOrBatchBarcode)
    {
        var results = await _context.StockItems
            .Include(s => s.Product)
            .ThenInclude(p => p.Category)
            .Include(s => s.Warehouse)
            .Where(s => s.LotNumber == lotOrBatchBarcode)
            .Select(s => new StockItemDto
            {
                ProductId = s.ProductId,
                ProductCode = s.Product.Code,
                ProductName = s.Product.Name,
                CategoryName = s.Product.Category.Name,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse.Name,
                CurrentQuantity = s.CurrentQuantity,
                LotNumber = s.LotNumber,
                ExpiryDate = s.ExpiryDate
            })
            .ToListAsync();

        return results;
    }
}
