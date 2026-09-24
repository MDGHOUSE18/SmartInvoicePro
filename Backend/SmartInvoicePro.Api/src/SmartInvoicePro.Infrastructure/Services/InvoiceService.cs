using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Invoice;
using SmartInvoicePro.Application.Exceptions;
using SmartInvoicePro.Application.Helpers;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Domain.Enums;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private readonly ICompanySettingsService _companySettingsService;

    public InvoiceService(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IPdfService pdfService,
        IEmailService emailService,
        ICompanySettingsService companySettingsService)
    {
        _context = context;
        _currentUser = currentUser;
        _auditService = auditService;
        _pdfService = pdfService;
        _emailService = emailService;
        _companySettingsService = companySettingsService;
    }

    public async Task<PagedResult<InvoiceListDto>> GetAllAsync(int page, int pageSize, string? status = null, Guid? customerId = null, string? search = null)
    {
        var query = _context.Invoices.AsNoTracking().Include(i => i.Customer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);
        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(term) ||
                i.Customer.CustomerName.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceListDto
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                CustomerName = i.Customer.CustomerName,
                Status = i.Status,
                GrandTotal = i.GrandTotal,
                BalanceAmount = i.BalanceAmount
            })
            .ToListAsync();

        return new PagedResult<InvoiceListDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<InvoiceDto> GetByIdAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);

        if (invoice == null) throw new NotFoundException("Invoice not found.");
        return Map(invoice);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto)
    {
        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null) throw new NotFoundException("Customer not found.");

        var company = await _context.CompanySettings.AsNoTracking().FirstOrDefaultAsync();
        var taxType = InvoiceCalculationHelper.DetermineTaxType(company?.State ?? "Karnataka", customer.State);
        var avgTax = await GetAverageTaxForItems(dto.Items);

        var totals = InvoiceCalculationHelper.Calculate(dto.Items, avgTax, taxType, dto.DiscountAmount);
        var invoiceNumber = await GenerateInvoiceNumberAsync();

        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            CustomerId = dto.CustomerId,
            Status = InvoiceStatuses.Draft,
            TaxType = totals.TaxType,
            Notes = dto.Notes,
            Subtotal = totals.Subtotal,
            CGSTAmount = totals.CGSTAmount,
            SGSTAmount = totals.SGSTAmount,
            IGSTAmount = totals.IGSTAmount,
            TaxAmount = totals.TaxAmount,
            DiscountAmount = dto.DiscountAmount,
            GrandTotal = totals.GrandTotal,
            AmountPaid = 0,
            BalanceAmount = totals.GrandTotal,
            CreatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow,
            Items = BuildItems(dto.Items, totals.TaxType, avgTax)
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Invoice", invoice.InvoiceId.ToString(), "Create", newValues: invoice.InvoiceNumber);

        return await GetByIdAsync(invoice.InvoiceId);
    }

    public async Task<InvoiceDto> UpdateAsync(Guid id, UpdateInvoiceDto dto)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);

        if (invoice == null) throw new NotFoundException("Invoice not found.");

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null) throw new NotFoundException("Customer not found.");

        var company = await _context.CompanySettings.AsNoTracking().FirstOrDefaultAsync();
        var taxType = InvoiceCalculationHelper.DetermineTaxType(company?.State ?? "Karnataka", customer.State);
        var avgTax = await GetAverageTaxForItems(dto.Items);
        var totals = InvoiceCalculationHelper.Calculate(dto.Items, avgTax, taxType, dto.DiscountAmount);

        _context.InvoiceItems.RemoveRange(invoice.Items);

        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.DueDate = dto.DueDate;
        invoice.CustomerId = dto.CustomerId;
        invoice.Status = dto.Status;
        invoice.Notes = dto.Notes;
        invoice.TaxType = totals.TaxType;
        invoice.Subtotal = totals.Subtotal;
        invoice.CGSTAmount = totals.CGSTAmount;
        invoice.SGSTAmount = totals.SGSTAmount;
        invoice.IGSTAmount = totals.IGSTAmount;
        invoice.TaxAmount = totals.TaxAmount;
        invoice.DiscountAmount = dto.DiscountAmount;
        invoice.GrandTotal = totals.GrandTotal;
        invoice.BalanceAmount = totals.GrandTotal - invoice.AmountPaid;
        invoice.UpdatedBy = _currentUser.UserId;
        invoice.UpdatedDate = DateTime.UtcNow;
        invoice.Items = BuildItems(dto.Items, totals.TaxType, avgTax);

        UpdateInvoiceStatus(invoice);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Invoice", id.ToString(), "Update");

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);

        if (invoice == null) throw new NotFoundException("Invoice not found.");
        if (invoice.Payments.Any())
            throw new ValidationException("Cannot delete invoice with payments.");

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Invoice", id.ToString(), "Delete");
    }

    public async Task<InvoiceDto> CloneAsync(Guid id)
    {
        var source = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);

        if (source == null) throw new NotFoundException("Invoice not found.");

        var dto = new CreateInvoiceDto
        {
            CustomerId = source.CustomerId,
            InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
            Notes = source.Notes,
            DiscountAmount = source.DiscountAmount,
            Items = source.Items.Select(x => new CreateInvoiceItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Discount = x.Discount
            }).ToList()
        };

        return await CreateAsync(dto);
    }

    public async Task<InvoiceDto> MarkAsPaidAsync(Guid id)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id);
        if (invoice == null) throw new NotFoundException("Invoice not found.");

        if (invoice.BalanceAmount <= 0)
            return await GetByIdAsync(id);

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            InvoiceId = id,
            PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AmountPaid = invoice.BalanceAmount,
            PaymentMethod = PaymentMethods.BankTransfer,
            BalanceAmount = 0,
            Notes = "Marked as paid",
            CreatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow
        };

        invoice.AmountPaid = invoice.GrandTotal;
        invoice.BalanceAmount = 0;
        invoice.Status = InvoiceStatuses.Paid;
        invoice.UpdatedBy = _currentUser.UserId;
        invoice.UpdatedDate = DateTime.UtcNow;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Invoice", id.ToString(), "MarkPaid");

        return await GetByIdAsync(id);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid id)
    {
        var invoice = await GetByIdAsync(id);
        var company = await _companySettingsService.GetAsync();
        return await _pdfService.GenerateInvoicePdfAsync(invoice, company);
    }

    public async Task SendInvoiceEmailAsync(Guid id)
    {
        var invoice = await _context.Invoices.Include(i => i.Customer).FirstOrDefaultAsync(i => i.InvoiceId == id);
        if (invoice == null) throw new NotFoundException("Invoice not found.");

        var pdf = await GeneratePdfAsync(id);
        await _emailService.SendEmailAsync(
            invoice.Customer.Email,
            $"Invoice {invoice.InvoiceNumber}",
            $"Please find attached invoice {invoice.InvoiceNumber}.",
            $"{invoice.InvoiceNumber}.pdf",
            pdf);

        if (invoice.Status == InvoiceStatuses.Draft)
        {
            invoice.Status = InvoiceStatuses.Sent;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<InvoiceSummaryDto> GetSummaryAsync()
    {
        var invoices = await _context.Invoices.AsNoTracking().ToListAsync();
        return new InvoiceSummaryDto
        {
            TotalInvoices = invoices.Count,
            TotalRevenue = invoices.Sum(i => i.GrandTotal),
            TotalOutstanding = invoices.Sum(i => i.BalanceAmount),
            DraftCount = invoices.Count(i => i.Status == InvoiceStatuses.Draft),
            SentCount = invoices.Count(i => i.Status == InvoiceStatuses.Sent),
            PaidCount = invoices.Count(i => i.Status == InvoiceStatuses.Paid),
            OverdueCount = invoices.Count(i => i.Status == InvoiceStatuses.Overdue)
        };
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";
        var last = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        var seq = 1;
        if (last != null && int.TryParse(last.Replace(prefix, ""), out var n))
            seq = n + 1;

        return $"{prefix}{seq:D4}";
    }

    private async Task<decimal> GetAverageTaxForItems(IReadOnlyList<CreateInvoiceItemDto> items)
    {
        var productIds = items.Where(i => i.ProductId.HasValue).Select(i => i.ProductId!.Value).Distinct().ToList();
        if (!productIds.Any()) return 18m;

        var products = await _context.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync();
        return products.Any() ? products.Average(p => p.TaxPercentage) : 18m;
    }

    private static List<InvoiceItem> BuildItems(IReadOnlyList<CreateInvoiceItemDto> items, string taxType, decimal taxPercentage)
    {
        var (cgst, sgst, igst) = InvoiceCalculationHelper.GetItemTaxPercentages(taxPercentage, taxType);
        return items.Select(item => new InvoiceItem
        {
            InvoiceItemId = Guid.NewGuid(),
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            CGSTPercentage = cgst,
            SGSTPercentage = sgst,
            IGSTPercentage = igst,
            Discount = item.Discount,
            LineTotal = InvoiceCalculationHelper.CalculateLineTotal(item.Quantity, item.UnitPrice, item.Discount)
        }).ToList();
    }

    internal static void UpdateInvoiceStatus(Invoice invoice)
    {
        if (invoice.Status == InvoiceStatuses.Cancelled) return;

        if (invoice.BalanceAmount <= 0)
            invoice.Status = InvoiceStatuses.Paid;
        else if (invoice.AmountPaid > 0)
            invoice.Status = InvoiceStatuses.PartiallyPaid;
        else if (invoice.DueDate < DateOnly.FromDateTime(DateTime.UtcNow) && invoice.Status != InvoiceStatuses.Draft)
            invoice.Status = InvoiceStatuses.Overdue;
    }

    private static InvoiceDto Map(Invoice i) => new()
    {
        InvoiceId = i.InvoiceId,
        InvoiceNumber = i.InvoiceNumber,
        InvoiceDate = i.InvoiceDate,
        DueDate = i.DueDate,
        CustomerId = i.CustomerId,
        CustomerName = i.Customer.CustomerName,
        CustomerCompanyName = i.Customer.CompanyName,
        CustomerAddress = i.Customer.Address,
        CustomerCity = i.Customer.City,
        CustomerState = i.Customer.State,
        CustomerCountry = i.Customer.Country,
        CustomerTaxNumber = i.Customer.TaxNumber,
        CustomerEmail = i.Customer.Email,
        CustomerPhone = i.Customer.Phone,
        Status = i.Status,
        TaxType = i.TaxType,
        CurrencyCode = i.CurrencyCode,
        Notes = i.Notes,
        Subtotal = i.Subtotal,
        CGSTAmount = i.CGSTAmount,
        SGSTAmount = i.SGSTAmount,
        IGSTAmount = i.IGSTAmount,
        TaxAmount = i.TaxAmount,
        DiscountAmount = i.DiscountAmount,
        GrandTotal = i.GrandTotal,
        AmountPaid = i.AmountPaid,
        BalanceAmount = i.BalanceAmount,
        Items = i.Items.Select(x => new InvoiceItemDto
        {
            InvoiceItemId = x.InvoiceItemId,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Description = x.Description,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            CGSTPercentage = x.CGSTPercentage,
            SGSTPercentage = x.SGSTPercentage,
            IGSTPercentage = x.IGSTPercentage,
            Discount = x.Discount,
            LineTotal = x.LineTotal
        }).ToList(),
        CreatedDate = i.CreatedDate
    };
}
