using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Notification;
using SmartInvoicePro.Application.Exceptions;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<NotificationDto>> GetForCurrentUserAsync(int page, int pageSize, bool? unreadOnly = null)
    {
        var userId = _currentUser.UserId;
        var query = _context.Notifications.AsNoTracking()
            .Where(n => n.UserId == null || n.UserId == userId);

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                ReferenceId = n.ReferenceId,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate
            })
            .ToListAsync();

        return new PagedResult<NotificationDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) throw new NotFoundException("Notification not found.");
        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = _currentUser.UserId;
        await _context.Notifications
            .Where(n => (n.UserId == null || n.UserId == userId) && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var userId = _currentUser.UserId;
        return await _context.Notifications
            .CountAsync(n => (n.UserId == null || n.UserId == userId) && !n.IsRead);
    }
}
