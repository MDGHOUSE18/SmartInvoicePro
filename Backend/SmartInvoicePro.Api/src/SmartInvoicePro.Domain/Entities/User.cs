namespace SmartInvoicePro.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public string FullName => $"{FirstName} {LastName}".Trim();
}
