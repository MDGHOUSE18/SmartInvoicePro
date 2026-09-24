namespace SmartInvoicePro.Application.DTOs.Product;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal CGSTPercentage { get; set; }
    public decimal SGSTPercentage { get; set; }
    public decimal IGSTPercentage { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedDate { get; set; }
}

public class CreateProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; } = 18m;
    public string Status { get; set; } = "Active";
}

public class UpdateProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public string Status { get; set; } = "Active";
}
