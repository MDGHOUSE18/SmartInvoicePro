namespace SmartInvoicePro.Domain.Entities;

public class Product : BaseEntity
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; } = 18m;
    public decimal CGSTPercentage { get; set; } = 9m;
    public decimal SGSTPercentage { get; set; } = 9m;
    public decimal IGSTPercentage { get; set; } = 18m;
    public string Status { get; set; } = "Active";
}
