using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Payment;
using SmartInvoicePro.Application.Exceptions;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Domain.Enums;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public PaymentService(ApplicationDbContext context, ICurrentUserService currentUser, IAuditService auditService)
    {
        _context = context;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<PagedResult<PaymentDto>> GetAllAsync(int page, int pageSize, Guid? invoiceId = null)
    {
        var query = _context.Payments.AsNoTracking()
            .Include(p => p.Invoice).ThenInclude(i => i.Customer)
            .AsQueryable();

        if (invoiceId.HasValue)
            query = query.Where(p => p.InvoiceId == invoiceId);

        var total = await query.CountAsync();
        var payments = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PaymentDto>
        {
            Items = payments.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<PaymentDto> GetByIdAsync(Guid id)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.PaymentId == id);

        if (payment == null) throw new NotFoundException("Payment not found.");
        return Map(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == dto.InvoiceId);
        if (invoice == null) throw new NotFoundException("Invoice not found.");
        if (invoice.Status == InvoiceStatuses.Cancelled)
            throw new ValidationException("Cannot record payment for cancelled invoice.");
        if (dto.AmountPaid > invoice.BalanceAmount)
            throw new ValidationException("Payment amount exceeds invoice balance.");

        invoice.AmountPaid += dto.AmountPaid;
        invoice.BalanceAmount = invoice.GrandTotal - invoice.AmountPaid;
        InvoiceService.UpdateInvoiceStatus(invoice);
        invoice.UpdatedBy = _currentUser.UserId;
        invoice.UpdatedDate = DateTime.UtcNow;

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            InvoiceId = dto.InvoiceId,
            PaymentDate = dto.PaymentDate,
            AmountPaid = dto.AmountPaid,
            PaymentMethod = dto.PaymentMethod,
            BalanceAmount = invoice.BalanceAmount,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            CreatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        _context.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = null,
            Title = "Payment received",
            Message = $"Payment of {dto.AmountPaid:N2} recorded for invoice {invoice.InvoiceNumber}.",
            Type = "PaymentReceived",
            ReferenceId = payment.PaymentId.ToString(),
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Payment", payment.PaymentId.ToString(), "Create");

        payment.Invoice = invoice;
        return Map(payment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.PaymentId == id);

        if (payment == null) throw new NotFoundException("Payment not found.");

        var invoice = payment.Invoice;
        invoice.AmountPaid -= payment.AmountPaid;
        invoice.BalanceAmount = invoice.GrandTotal - invoice.AmountPaid;
        InvoiceService.UpdateInvoiceStatus(invoice);

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Payment", id.ToString(), "Delete");
    }

    private static PaymentDto Map(Payment p) => new()
    {
        PaymentId = p.PaymentId,
        InvoiceId = p.InvoiceId,
        InvoiceNumber = p.Invoice.InvoiceNumber,
        PaymentDate = p.PaymentDate,
        AmountPaid = p.AmountPaid,
        PaymentMethod = p.PaymentMethod,
        BalanceAmount = p.BalanceAmount,
        ReferenceNumber = p.ReferenceNumber,
        Notes = p.Notes,
        CreatedDate = p.CreatedDate
    };
}
