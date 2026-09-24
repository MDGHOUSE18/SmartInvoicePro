using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Invoice;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Constants;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService) => _invoiceService = invoiceService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceListDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null, [FromQuery] Guid? customerId = null, [FromQuery] string? search = null)
    {
        var result = await _invoiceService.GetAllAsync(page, pageSize, status, customerId, search);
        return Ok(ApiResponse<PagedResult<InvoiceListDto>>.Ok(result));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<InvoiceSummaryDto>>> GetSummary()
    {
        var result = await _invoiceService.GetSummaryAsync();
        return Ok(ApiResponse<InvoiceSummaryDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetById(Guid id)
    {
        var result = await _invoiceService.GetByIdAsync(id);
        return Ok(ApiResponse<InvoiceDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Create([FromBody] CreateInvoiceDto dto)
    {
        var result = await _invoiceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.InvoiceId }, ApiResponse<InvoiceDto>.Ok(result, "Invoice created"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Update(Guid id, [FromBody] UpdateInvoiceDto dto)
    {
        var result = await _invoiceService.UpdateAsync(id, dto);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Invoice updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _invoiceService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Invoice deleted"));
    }

    [HttpPost("{id:guid}/clone")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Clone(Guid id)
    {
        var result = await _invoiceService.CloneAsync(id);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Invoice cloned"));
    }

    [HttpPost("{id:guid}/mark-paid")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> MarkAsPaid(Guid id)
    {
        var result = await _invoiceService.MarkAsPaidAsync(id);
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "Invoice marked as paid"));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id)
    {
        var pdf = await _invoiceService.GeneratePdfAsync(id);
        return File(pdf, "application/pdf", $"invoice-{id}.pdf");
    }

    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<ApiResponse>> SendEmail(Guid id)
    {
        await _invoiceService.SendInvoiceEmailAsync(id);
        return Ok(ApiResponse.Ok("Invoice email queued"));
    }
}
