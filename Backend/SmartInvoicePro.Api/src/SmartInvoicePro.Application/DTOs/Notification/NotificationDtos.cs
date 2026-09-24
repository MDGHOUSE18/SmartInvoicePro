namespace SmartInvoicePro.Application.DTOs.Notification;

public class NotificationDto
{
    public Guid NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class MarkNotificationReadDto
{
    public bool IsRead { get; set; } = true;
}
