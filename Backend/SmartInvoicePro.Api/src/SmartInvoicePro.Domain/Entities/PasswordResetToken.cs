namespace SmartInvoicePro.Domain.Entities;

public class PasswordResetToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
