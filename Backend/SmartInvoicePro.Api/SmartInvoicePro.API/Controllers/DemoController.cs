using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Constants;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Admin)]
public class DemoController : ControllerBase
{
    private readonly IDataSeeder _dataSeeder;

    public DemoController(IDataSeeder dataSeeder) => _dataSeeder = dataSeeder;

    [HttpPost("reset")]
    public async Task<ActionResult<ApiResponse>> ResetDemoData()
    {
        await _dataSeeder.ResetDemoDataAsync();
        return Ok(ApiResponse.Ok("Demo data has been reset successfully"));
    }
}
