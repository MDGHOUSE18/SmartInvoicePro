namespace SmartInvoicePro.Domain.Entities;

public class EmailLog
{
    public Guid EmailLogId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? AttachmentName { get; set; }
    public string Status { get; set; } = "Logged";
    public Guid? SentBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
