using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.CompanySettings;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Constants;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanySettingsController : ControllerBase
{
    private readonly ICompanySettingsService _companySettingsService;

    public CompanySettingsController(ICompanySettingsService companySettingsService)
        => _companySettingsService = companySettingsService;

    [HttpGet]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
    public async Task<ActionResult<ApiResponse<CompanySettingsDto>>> Get()
    {
        var result = await _companySettingsService.GetAsync();
        return Ok(ApiResponse<CompanySettingsDto>.Ok(result));
    }

    [HttpPut]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse<CompanySettingsDto>>> Update([FromBody] UpdateCompanySettingsDto dto)
    {
        var result = await _companySettingsService.UpdateAsync(dto);
        return Ok(ApiResponse<CompanySettingsDto>.Ok(result, "Settings updated"));
    }
}
