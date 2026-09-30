namespace KasapOtomasyon.Domain.Entities;

public class Customer : BaseEntity
{
    public string Code { get; set; } = string.Empty; // CAR-001
    public string Name { get; set; } = string.Empty; // Müşteri / Firma Adı
    public string? TaxNumberOrId { get; set; } // VKN / TCKN
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; } // SMS Hatırlatma için
    public string? Email { get; set; }
    
    public decimal CurrentBalance { get; set; } // Pozitif: Alacağımız var (Borçlu), Negatif: Fazla ödeme
    public decimal CreditLimit { get; set; } = 10000m; // Maksimum borçlanma limiti
    public int PaymentTermDays { get; set; } = 30; // Vade günü
    public int? PriceListId { get; set; }
    public virtual PriceList? PriceList { get; set; }
    public bool IsCompany { get; set; }

    public virtual ICollection<CustomerTransaction> Transactions { get; set; } = new List<CustomerTransaction>();
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class CustomerTransaction : BaseEntity
{
    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string TransactionType { get; set; } = "Satis"; // "Satis", "Tahsilat", "AlacakDekontu", "BorcDekontu", "Iade"
    public decimal Debit { get; set; } // Borç (Müşterinin borçlandığı tutar)
    public decimal Credit { get; set; } // Alacak (Müşterinin ödediği tutar)
    public decimal BalanceAfter { get; set; }
    
    public string? Description { get; set; }
    public string? DocumentNumber { get; set; } // Fatura, İrsaliye, Makbuz No
    public int? SaleId { get; set; }
    public DateTime? DueDate { get; set; } // Vade Tarihi
    public bool IsPaid { get; set; }
}
