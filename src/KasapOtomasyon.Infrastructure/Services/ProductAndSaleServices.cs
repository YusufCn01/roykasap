using KasapOtomasyon.Application.DTOs;
using KasapOtomasyon.Application.Interfaces;
using KasapOtomasyon.Domain.Entities;
using KasapOtomasyon.Domain.Enums;
using KasapOtomasyon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasapOtomasyon.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly KasapDbContext _context;

    public ProductService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.UnitOfMeasure)
            .Include(p => p.Barcodes)
            .Include(p => p.StockItems)
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                UnitId = p.UnitOfMeasureId,
                UnitName = p.UnitOfMeasure.Name,
                UnitCode = p.UnitOfMeasure.Code,
                ProductType = p.ProductType,
                VatRate = p.VatRate,
                SalePrice = p.SalePrice,
                CostPrice = p.CostPrice,
                CurrentStock = p.StockItems.Sum(s => s.CurrentQuantity),
                ShelfLifeDays = p.ShelfLifeDays,
                IsQuickButton = p.IsQuickButton,
                ButtonColor = p.ButtonColor ?? p.Category.ColorCode,
                DisplayOrder = p.DisplayOrder,
                PluCode = p.PluCode,
                Barcodes = p.Barcodes.Select(b => b.Barcode).ToList()
            })
            .ToListAsync();
    }

    public async Task<List<ProductDto>> GetQuickButtonProductsAsync()
    {
        var all = await GetAllProductsAsync();
        return all.Where(p => p.IsQuickButton).OrderBy(p => p.DisplayOrder).ToList();
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var all = await GetAllProductsAsync();
        return all.Where(p => p.CategoryId == categoryId).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var all = await GetAllProductsAsync();
        return all.FirstOrDefault(p => p.Id == id);
    }

    public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode)
    {
        barcode = barcode.Trim();
        var all = await GetAllProductsAsync();

        // Direct barcode match
        var match = all.FirstOrDefault(p => p.Barcodes.Contains(barcode) || p.Code.Equals(barcode, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match;

        // PLU match (if barcode is embedded scale barcode e.g. 2000001012503)
        if (barcode.Length == 13 && barcode.StartsWith("20"))
        {
            var plu = barcode.Substring(2, 5);
            return all.FirstOrDefault(p => p.PluCode == plu || p.PluCode?.TrimStart('0') == plu.TrimStart('0'));
        }

        return null;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
    }

    public async Task<List<UnitOfMeasure>> GetAllUnitsAsync()
    {
        return await _context.UnitsOfMeasure.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<ProductDto> CreateOrUpdateProductAsync(Product product, List<string> barcodes)
    {
        if (product.Id == 0)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            foreach (var b in barcodes.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                await _context.BarcodeDefinitions.AddAsync(new BarcodeDefinition
                {
                    ProductId = product.Id,
                    Barcode = b.Trim(),
                    BarcodeType = b.Length == 13 ? BarcodeType.EAN13 : BarcodeType.Code128
                });
            }
        }
        else
        {
            _context.Products.Update(product);

            var existingBarcodes = await _context.BarcodeDefinitions.Where(b => b.ProductId == product.Id).ToListAsync();
            _context.BarcodeDefinitions.RemoveRange(existingBarcodes);

            foreach (var b in barcodes.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                await _context.BarcodeDefinitions.AddAsync(new BarcodeDefinition
                {
                    ProductId = product.Id,
                    Barcode = b.Trim(),
                    BarcodeType = b.Length == 13 ? BarcodeType.EAN13 : BarcodeType.Code128
                });
            }
        }

        await _context.SaveChangesAsync();
        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        product.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

public class SaleService : ISaleService
{
    private readonly KasapDbContext _context;
    private static readonly List<ParkedSaleDto> ParkedSalesInMemory = new();

    public SaleService(KasapDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> CompleteSaleAsync(CompleteSaleRequestDto request)
    {
        if (request.Items == null || !request.Items.Any())
            throw new InvalidOperationException("Sepet boş, satış tamamlanamaz.");

        var defaultWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.IsDefault) 
                                ?? await _context.Warehouses.FirstAsync();

        var subTotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var discountTotal = request.DiscountTotal + request.Items.Sum(i => i.DiscountAmount);
        var grandTotal = Math.Max(0, subTotal - discountTotal);
        var vatTotal = request.Items.Sum(i => Math.Round(i.TotalPrice * (i.VatRate / 100m), 2));

        var receiptNumber = $"FIS-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(10, 99)}";

        var sale = new Sale
        {
            ReceiptNumber = receiptNumber,
            SaleDate = DateTime.UtcNow,
            CashierId = request.CashierId,
            CashierName = request.CashierName,
            CustomerId = request.CustomerId,
            SubTotal = subTotal,
            DiscountTotal = discountTotal,
            VatTotal = vatTotal,
            GrandTotal = grandTotal,
            PaidAmount = request.Payments.Sum(p => p.Amount),
            ChangeAmount = Math.Max(0, request.Payments.Sum(p => p.Amount) - grandTotal),
            PrimaryPaymentType = request.Payments.FirstOrDefault()?.PaymentType ?? PaymentType.Nakit,
            Status = SaleStatus.Tamamlandi,
            TerminalId = request.TerminalId ?? "TERM-01",
            Notes = request.Notes
        };

        foreach (var item in request.Items)
        {
            var saleItem = new SaleItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Barcode = item.Barcode,
                Quantity = item.Quantity,
                UnitName = item.UnitName,
                UnitPrice = item.UnitPrice,
                VatRate = item.VatRate,
                VatAmount = Math.Round(item.TotalPrice * (item.VatRate / 100m), 2),
                DiscountPercent = item.DiscountPercent,
                DiscountAmount = item.DiscountAmount,
                TotalAmount = item.TotalPrice,
                TraceabilityLotNo = item.TraceabilityLotNo
            };
            sale.Items.Add(saleItem);

            // Deduct stock
            var stockItem = await _context.StockItems.FirstOrDefaultAsync(s => s.WarehouseId == defaultWarehouse.Id && s.ProductId == item.ProductId);
            if (stockItem != null)
            {
                stockItem.CurrentQuantity -= item.Quantity;
            }
            else
            {
                await _context.StockItems.AddAsync(new StockItem
                {
                    WarehouseId = defaultWarehouse.Id,
                    ProductId = item.ProductId,
                    CurrentQuantity = -item.Quantity
                });
            }

            // Record movement
            await _context.StockMovements.AddAsync(new StockMovement
            {
                WarehouseId = defaultWarehouse.Id,
                ProductId = item.ProductId,
                MovementType = StockMovementType.Satis,
                Quantity = -item.Quantity,
                UnitPrice = item.UnitPrice,
                ReferenceNumber = receiptNumber,
                Description = $"POS Satış Fişi: {receiptNumber}",
                MovementDate = DateTime.UtcNow,
                UserId = request.CashierId
            });
        }

        foreach (var p in request.Payments)
        {
            sale.Payments.Add(new SalePayment
            {
                PaymentType = p.PaymentType,
                Amount = p.Amount,
                BankOrCreditCardName = p.BankOrCardName,
                InstallmentCount = p.Installments,
                CommissionRate = p.CommissionRate,
                CommissionAmount = Math.Round(p.Amount * (p.CommissionRate / 100m), 2)
            });

            // If Customer Cari account, update balance
            if (p.PaymentType == PaymentType.CariHesap && request.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(request.CustomerId.Value);
                if (customer != null)
                {
                    customer.CurrentBalance += p.Amount;
                    await _context.CustomerTransactions.AddAsync(new CustomerTransaction
                    {
                        CustomerId = customer.Id,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = "Satis",
                        Debit = p.Amount,
                        Credit = 0,
                        BalanceAfter = customer.CurrentBalance,
                        Description = $"Veresiye Satış Fiş No: {receiptNumber}",
                        DueDate = DateTime.UtcNow.AddDays(customer.PaymentTermDays),
                        IsPaid = false
                    });
                }
            }
        }

        await _context.Sales.AddAsync(sale);
        await _context.SaveChangesAsync();

        return sale;
    }

    public Task<ParkedSaleDto> ParkSaleAsync(CompleteSaleRequestDto request, string? note)
    {
        var park = new ParkedSaleDto
        {
            ParkId = Guid.NewGuid().ToString(),
            ParkedAt = DateTime.UtcNow,
            ParkNote = note ?? $"Bekletilen Satış ({DateTime.Now:HH:mm})",
            GrandTotal = request.Items.Sum(i => i.TotalPrice),
            Items = request.Items
        };
        ParkedSalesInMemory.Add(park);
        return Task.FromResult(park);
    }

    public Task<List<ParkedSaleDto>> GetParkedSalesAsync()
    {
        return Task.FromResult(ParkedSalesInMemory.ToList());
    }

    public Task<bool> RemoveParkedSaleAsync(string parkId)
    {
        var found = ParkedSalesInMemory.FirstOrDefault(p => p.ParkId == parkId);
        if (found != null)
        {
            ParkedSalesInMemory.Remove(found);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public async Task<Sale?> GetSaleByReceiptNumberAsync(string receiptNumber)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.ReceiptNumber == receiptNumber);
    }

    public async Task<bool> RefundSaleAsync(int saleId, string reason, int userId)
    {
        var sale = await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == saleId);
        if (sale == null || sale.Status == SaleStatus.Iade) return false;

        sale.Status = SaleStatus.Iade;
        sale.Notes = $"{sale.Notes} | İade Sebebi: {reason}";

        var defaultWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.IsDefault) ?? await _context.Warehouses.FirstAsync();

        // Restore stock
        foreach (var item in sale.Items)
        {
            var stock = await _context.StockItems.FirstOrDefaultAsync(s => s.WarehouseId == defaultWarehouse.Id && s.ProductId == item.ProductId);
            if (stock != null)
            {
                stock.CurrentQuantity += item.Quantity;
            }

            await _context.StockMovements.AddAsync(new StockMovement
            {
                WarehouseId = defaultWarehouse.Id,
                ProductId = item.ProductId,
                MovementType = StockMovementType.Iade,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                ReferenceNumber = sale.ReceiptNumber,
                Description = $"İade: {reason}",
                MovementDate = DateTime.UtcNow,
                UserId = userId
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
