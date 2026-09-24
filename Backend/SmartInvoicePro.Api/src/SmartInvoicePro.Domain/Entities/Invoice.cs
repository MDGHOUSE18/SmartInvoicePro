namespace SmartInvoicePro.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string Status { get; set; } = "Draft";
    public string TaxType { get; set; } = "CGST_SGST";
    public string CurrencyCode { get; set; } = "INR";
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceAmount { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
