using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.AuditLog;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Constants;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Admin)]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService) => _auditService = auditService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAll([FromQuery] AuditLogFilterDto filter)
    {
        var result = await _auditService.GetAllAsync(filter);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }
}
