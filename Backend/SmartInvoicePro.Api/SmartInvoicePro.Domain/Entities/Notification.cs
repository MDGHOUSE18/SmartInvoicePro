namespace SmartInvoicePro.Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; set; }
    public Guid? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
