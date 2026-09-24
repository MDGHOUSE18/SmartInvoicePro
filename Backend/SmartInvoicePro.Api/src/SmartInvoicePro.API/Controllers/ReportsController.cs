using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Reports;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Constants;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Admin)]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    [HttpGet("sales")]
    public async Task<ActionResult<ApiResponse<SalesReportDto>>> Sales([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetSalesReportAsync(filter);
        return Ok(ApiResponse<SalesReportDto>.Ok(result));
    }

    [HttpGet("tax")]
    public async Task<ActionResult<ApiResponse<TaxReportDto>>> Tax([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetTaxReportAsync(filter);
        return Ok(ApiResponse<TaxReportDto>.Ok(result));
    }

    [HttpGet("customers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CustomerReportDto>>>> Customers([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetCustomerReportAsync(filter);
        return Ok(ApiResponse<IReadOnlyList<CustomerReportDto>>.Ok(result));
    }

    [HttpGet("payments")]
    public async Task<ActionResult<ApiResponse<PaymentReportDto>>> Payments([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetPaymentReportAsync(filter);
        return Ok(ApiResponse<PaymentReportDto>.Ok(result));
    }

    [HttpGet("sales/export")]
    public async Task<IActionResult> ExportSales([FromQuery] ReportFilterDto filter)
    {
        var bytes = await _reportService.ExportSalesReportExcelAsync(filter);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "sales-report.xlsx");
    }

    [HttpGet("tax/export")]
    public async Task<IActionResult> ExportTax([FromQuery] ReportFilterDto filter)
    {
        var bytes = await _reportService.ExportTaxReportExcelAsync(filter);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "tax-report.xlsx");
    }
}
