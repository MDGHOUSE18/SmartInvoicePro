using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.DTOs.Reports;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IExcelExportService _excelExportService;

    public ReportService(ApplicationDbContext context, IExcelExportService excelExportService)
    {
        _context = context;
        _excelExportService = excelExportService;
    }

    public async Task<SalesReportDto> GetSalesReportAsync(ReportFilterDto filter)
    {
        var (from, to) = GetDateRange(filter);
        var query = BuildInvoiceQuery(filter, from, to);
        var invoices = await query.ToListAsync();

        return new SalesReportDto
        {
            FromDate = from,
            ToDate = to,
            TotalInvoices = invoices.Count,
            TotalSales = invoices.Sum(i => i.GrandTotal),
            TotalTax = invoices.Sum(i => i.TaxAmount),
            TotalDiscount = invoices.Sum(i => i.DiscountAmount),
            TotalCollected = invoices.Sum(i => i.AmountPaid),
            TotalOutstanding = invoices.Sum(i => i.BalanceAmount),
            Lines = invoices.Select(i => new SalesReportLineDto
            {
                InvoiceDate = i.InvoiceDate,
                InvoiceNumber = i.InvoiceNumber,
                CustomerName = i.Customer.CustomerName,
                Status = i.Status,
                Subtotal = i.Subtotal,
                TaxAmount = i.TaxAmount,
                GrandTotal = i.GrandTotal,
                AmountPaid = i.AmountPaid
            }).ToList()
        };
    }

    public async Task<TaxReportDto> GetTaxReportAsync(ReportFilterDto filter)
    {
        var (from, to) = GetDateRange(filter);
        var query = BuildInvoiceQuery(filter, from, to);
        var invoices = await query.ToListAsync();

        return new TaxReportDto
        {
            FromDate = from,
            ToDate = to,
            TotalCGST = invoices.Sum(i => i.CGSTAmount),
            TotalSGST = invoices.Sum(i => i.SGSTAmount),
            TotalIGST = invoices.Sum(i => i.IGSTAmount),
            TotalTax = invoices.Sum(i => i.TaxAmount),
            Lines = invoices.Select(i => new TaxReportLineDto
            {
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                CustomerName = i.Customer.CustomerName,
                TaxType = i.TaxType,
                CGSTAmount = i.CGSTAmount,
                SGSTAmount = i.SGSTAmount,
                IGSTAmount = i.IGSTAmount,
                TaxAmount = i.TaxAmount
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<CustomerReportDto>> GetCustomerReportAsync(ReportFilterDto filter)
    {
        var (from, to) = GetDateRange(filter);
        var customers = await _context.Customers.AsNoTracking()
            .Include(c => c.Invoices)
            .ToListAsync();

        return customers
            .Select(c =>
            {
                var invoices = c.Invoices.Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to).ToList();
                return new CustomerReportDto
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    InvoiceCount = invoices.Count,
                    TotalBilled = invoices.Sum(i => i.GrandTotal),
                    TotalPaid = invoices.Sum(i => i.AmountPaid),
                    Outstanding = invoices.Sum(i => i.BalanceAmount)
                };
            })
            .Where(r => r.InvoiceCount > 0)
            .OrderByDescending(r => r.TotalBilled)
            .ToList();
    }

    public async Task<PaymentReportDto> GetPaymentReportAsync(ReportFilterDto filter)
    {
        var (from, to) = GetDateRange(filter);
        var payments = await _context.Payments.AsNoTracking()
            .Include(p => p.Invoice).ThenInclude(i => i.Customer)
            .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return new PaymentReportDto
        {
            FromDate = from,
            ToDate = to,
            TotalCollected = payments.Sum(p => p.AmountPaid),
            Lines = payments.Select(p => new PaymentReportLineDto
            {
                PaymentDate = p.PaymentDate,
                InvoiceNumber = p.Invoice.InvoiceNumber,
                CustomerName = p.Invoice.Customer.CustomerName,
                PaymentMethod = p.PaymentMethod,
                AmountPaid = p.AmountPaid,
                ReferenceNumber = p.ReferenceNumber
            }).ToList()
        };
    }

    public async Task<byte[]> ExportSalesReportExcelAsync(ReportFilterDto filter)
    {
        var report = await GetSalesReportAsync(filter);
        return await _excelExportService.ExportSalesReportAsync(report);
    }

    public async Task<byte[]> ExportTaxReportExcelAsync(ReportFilterDto filter)
    {
        var report = await GetTaxReportAsync(filter);
        return await _excelExportService.ExportTaxReportAsync(report);
    }

    private IQueryable<Domain.Entities.Invoice> BuildInvoiceQuery(ReportFilterDto filter, DateOnly from, DateOnly to)
    {
        var query = _context.Invoices.AsNoTracking()
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(i => i.Status == filter.Status);
        if (filter.CustomerId.HasValue)
            query = query.Where(i => i.CustomerId == filter.CustomerId);

        return query.OrderBy(i => i.InvoiceDate);
    }

    private static (DateOnly From, DateOnly To) GetDateRange(ReportFilterDto filter)
    {
        var to = filter.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = filter.FromDate ?? to.AddMonths(-1);
        return (from, to);
    }
}
