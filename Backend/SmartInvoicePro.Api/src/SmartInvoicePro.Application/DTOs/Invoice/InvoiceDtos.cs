namespace SmartInvoicePro.Application.DTOs.Invoice;

public class InvoiceDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCompanyName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerState { get; set; }
    public string? CustomerCountry { get; set; }
    public string? CustomerTaxNumber { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
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
    public IReadOnlyList<InvoiceItemDto> Items { get; set; } = [];
    public DateTime CreatedDate { get; set; }
}

public class InvoiceListDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal BalanceAmount { get; set; }
}

public class InvoiceItemDto
{
    public Guid? InvoiceItemId { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CGSTPercentage { get; set; }
    public decimal SGSTPercentage { get; set; }
    public decimal IGSTPercentage { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateInvoiceDto
{
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? Notes { get; set; }
    public decimal DiscountAmount { get; set; }
    public IReadOnlyList<CreateInvoiceItemDto> Items { get; set; } = [];
}

public class CreateInvoiceItemDto
{
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}

public class UpdateInvoiceDto
{
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal DiscountAmount { get; set; }
    public IReadOnlyList<CreateInvoiceItemDto> Items { get; set; } = [];
}

public class InvoiceSummaryDto
{
    public int TotalInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int DraftCount { get; set; }
    public int SentCount { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }
}
