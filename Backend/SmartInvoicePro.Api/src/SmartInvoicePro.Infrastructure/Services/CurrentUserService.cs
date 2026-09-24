using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartInvoicePro.Application.Interfaces;

namespace SmartInvoicePro.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public Guid? UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var userId) ? userId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public bool IsInRole(string role)
        => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}
