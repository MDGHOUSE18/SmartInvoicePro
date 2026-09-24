using Microsoft.AspNetCore.Identity;
using SmartInvoicePro.Domain.Entities;

namespace SmartInvoicePro.Infrastructure.Security;

public class PasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password) => _hasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        => _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) != PasswordVerificationResult.Failed;
}
