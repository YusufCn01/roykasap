using KasapOtomasyon.Domain.Enums;

namespace KasapOtomasyon.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = "KG";
    public string UnitCode { get; set; } = "KG";
    public ProductType ProductType { get; set; }
    public decimal VatRate { get; set; }
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CurrentStock { get; set; }
    public int ShelfLifeDays { get; set; }
    public bool IsQuickButton { get; set; }
    public string? ButtonColor { get; set; }
    public int DisplayOrder { get; set; }
    public string? PluCode { get; set; }
    public List<string> Barcodes { get; set; } = new();
}

public class EmbeddedWeightBarcodeDto
{
    public bool IsValid { get; set; }
    public string RawBarcode { get; set; } = string.Empty;
    public string PluCode { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal PriceTotal { get; set; }
    public bool IsWeightEmbedded { get; set; } // True = weight embedded, False = price embedded
}

public class PosCartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? PluCode { get; set; }
    public decimal Quantity { get; set; } = 1.0m;
    public string UnitName { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; } = 1.0m;
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice => Math.Round((Quantity * UnitPrice) - DiscountAmount, 2);
    public string? TraceabilityLotNo { get; set; }
}

public class SplitPaymentDto
{
    public PaymentType PaymentType { get; set; } = PaymentType.Nakit;
    public decimal Amount { get; set; }
    public string? BankOrCardName { get; set; }
    public int Installments { get; set; } = 1;
    public decimal CommissionRate { get; set; }
}

public class CompleteSaleRequestDto
{
    public List<PosCartItemDto> Items { get; set; } = new();
    public List<SplitPaymentDto> Payments { get; set; } = new();
    public int? CustomerId { get; set; }
    public int? CashierId { get; set; }
    public string CashierName { get; set; } = "Kasiyer";
    public decimal DiscountTotal { get; set; }
    public string? Notes { get; set; }
    public string? TerminalId { get; set; }
}

public class ParkedSaleDto
{
    public string ParkId { get; set; } = Guid.NewGuid().ToString();
    public DateTime ParkedAt { get; set; } = DateTime.UtcNow;
    public string? CustomerName { get; set; }
    public string? ParkNote { get; set; }
    public decimal GrandTotal { get; set; }
    public List<PosCartItemDto> Items { get; set; } = new();
}
