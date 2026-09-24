using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.AuditLog;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AuditLogDto>> GetAllAsync(AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
            query = query.Where(x => x.EntityName == filter.EntityName);
        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(x => x.Action == filter.Action);
        if (filter.UserId.HasValue)
            query = query.Where(x => x.UserId == filter.UserId);
        if (filter.FromDate.HasValue)
            query = query.Where(x => x.CreatedDate >= filter.FromDate);
        if (filter.ToDate.HasValue)
            query = query.Where(x => x.CreatedDate <= filter.ToDate);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new AuditLogDto
            {
                AuditLogId = x.AuditLogId,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Action = x.Action,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                UserId = x.UserId,
                UserEmail = x.UserEmail,
                IpAddress = x.IpAddress,
                CreatedDate = x.CreatedDate
            })
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task LogAsync(string entityName, string entityId, string action, string? oldValues = null, string? newValues = null)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = _currentUser.UserId,
            UserEmail = _currentUser.Email,
            IpAddress = _currentUser.IpAddress,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
