using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartInvoicePro.Application.DTOs.Auth;
using SmartInvoicePro.Application.Exceptions;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Infrastructure.Data;
using SmartInvoicePro.Infrastructure.Security;

namespace SmartInvoicePro.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasherService _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;
    private readonly string _frontendUrl;

    public AuthService(
        ApplicationDbContext context,
        PasswordHasherService passwordHasher,
        JwtTokenService jwtTokenService,
        ICurrentUserService currentUser,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _currentUser = currentUser;
        _emailService = emailService;
        _frontendUrl = configuration["App:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new ValidationException("Email is already registered.");

        var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Staff")
            ?? throw new ValidationException("Staff role not configured.");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = staffRole.RoleId });
        await _context.SaveChangesAsync();

        return await LoginAsync(new LoginRequestDto { Email = request.Email, Password = request.Password });
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null || !_passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        var (token, expires) = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiry();
        user.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            User = MapUser(user, roles)
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.IsActive);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        var (token, expires) = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiry();
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            User = MapUser(user, roles)
        };
    }

    public async Task<UserDto> GetCurrentUserAsync()
    {
        if (!_currentUser.UserId.HasValue)
            throw new UnauthorizedException("User not authenticated.");

        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == _currentUser.UserId);

        if (user == null)
            throw new NotFoundException("User not found.");

        return MapUser(user, user.UserRoles.Select(ur => ur.Role.RoleName).ToList());
    }

    public async Task<string?> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return null;

        var tokenValue = Guid.NewGuid().ToString("N");
        var token = new PasswordResetToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedDate = DateTime.UtcNow
        };
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        var resetLink = $"{_frontendUrl}/auth/reset-password?token={tokenValue}";
        await _emailService.SendEmailAsync(
            user.Email,
            "SmartInvoice Pro — Password reset",
            $"Use this link to reset your password (valid 24 hours):\n{resetLink}\n\nIf you did not request this, ignore this email.");

        return tokenValue;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        if (resetToken == null)
            throw new ValidationException("Invalid or expired reset token.");

        resetToken.User.PasswordHash = _passwordHasher.HashPassword(resetToken.User, request.NewPassword);
        resetToken.IsUsed = true;
        resetToken.User.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto request)
    {
        if (!_currentUser.UserId.HasValue)
            throw new UnauthorizedException("User not authenticated.");

        var user = await _context.Users.FindAsync(_currentUser.UserId.Value);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (!_passwordHasher.VerifyPassword(user, user.PasswordHash, request.CurrentPassword))
            throw new ValidationException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static UserDto MapUser(User user, IReadOnlyList<string> roles) => new()
    {
        UserId = user.UserId,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FullName,
        Phone = user.Phone,
        Roles = roles
    };
}
