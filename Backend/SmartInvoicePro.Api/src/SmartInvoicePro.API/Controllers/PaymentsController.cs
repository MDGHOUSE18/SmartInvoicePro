using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Payment;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Constants;

namespace SmartInvoicePro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Staff}")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? invoiceId = null)
    {
        var result = await _paymentService.GetAllAsync(page, pageSize, invoiceId);
        return Ok(ApiResponse<PagedResult<PaymentDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(Guid id)
    {
        var result = await _paymentService.GetByIdAsync(id);
        return Ok(ApiResponse<PaymentDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create([FromBody] CreatePaymentDto dto)
    {
        var result = await _paymentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.PaymentId }, ApiResponse<PaymentDto>.Ok(result, "Payment recorded"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _paymentService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Payment deleted"));
    }
}
