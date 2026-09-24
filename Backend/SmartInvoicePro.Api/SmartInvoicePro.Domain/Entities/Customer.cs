namespace SmartInvoicePro.Domain.Entities;

public class Customer : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Country { get; set; } = "India";
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
