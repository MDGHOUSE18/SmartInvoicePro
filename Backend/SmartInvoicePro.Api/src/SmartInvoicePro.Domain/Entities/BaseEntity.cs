namespace SmartInvoicePro.Domain.Entities;

public abstract class BaseEntity
{
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
