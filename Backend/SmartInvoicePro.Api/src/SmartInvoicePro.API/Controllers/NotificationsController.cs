using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Notification;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Constants;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
        => _notificationService = notificationService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? unreadOnly = null)
    {
        var result = await _notificationService.GetForCurrentUserAsync(page, pageSize, unreadOnly);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> UnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync();
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse>> MarkRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(ApiResponse.Ok("Notification marked as read"));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllRead()
    {
        await _notificationService.MarkAllAsReadAsync();
        return Ok(ApiResponse.Ok("All notifications marked as read"));
    }
}
