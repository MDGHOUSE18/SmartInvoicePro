using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.DTOs.Dashboard;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Enums;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private const int TrendMonths = 12;

    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context) => _context = context;

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var invoices = await _context.Invoices.AsNoTracking().ToListAsync();
        var payments = await _context.Payments.AsNoTracking().ToListAsync();
        var customers = await _context.Customers.AsNoTracking().CountAsync();
        var activeProducts = await _context.Products.AsNoTracking().CountAsync(p => p.Status == "Active");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var billable = invoices.Where(i => i.Status != InvoiceStatuses.Cancelled).ToList();
        var totalRevenue = billable.Sum(i => i.GrandTotal);
        var totalCollected = payments.Sum(p => p.AmountPaid);

        var revenueThisMonth = billable.Where(i => i.InvoiceDate >= startOfMonth).Sum(i => i.GrandTotal);
        var revenueLastMonth = billable
            .Where(i => i.InvoiceDate >= startOfLastMonth && i.InvoiceDate < startOfMonth)
            .Sum(i => i.GrandTotal);

        var monthlyRevenue = Enumerable.Range(0, TrendMonths)
            .Select(offset => startOfMonth.AddMonths(offset - (TrendMonths - 1)))
            .Select(month =>
            {
                var next = month.AddMonths(1);
                var monthInvoices = billable.Where(i => i.InvoiceDate >= month && i.InvoiceDate < next).ToList();
                return new MonthlyRevenueDto
                {
                    Month = month.ToString("MMM"),
                    Year = month.Year,
                    Revenue = monthInvoices.Sum(i => i.GrandTotal),
                    Collected = payments.Where(p => p.PaymentDate >= month && p.PaymentDate < next).Sum(p => p.AmountPaid),
                    InvoiceCount = monthInvoices.Count
                };
            })
            .ToList();

        var statusBreakdown = invoices
            .GroupBy(i => i.Status)
            .Select(g => new InvoiceStatusBreakdownDto
            {
                Status = g.Key,
                Count = g.Count(),
                Amount = g.Sum(i => i.GrandTotal)
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        var paymentMethodBreakdown = payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodBreakdownDto
            {
                Method = g.Key,
                Count = g.Count(),
                Amount = g.Sum(p => p.AmountPaid)
            })
            .OrderByDescending(p => p.Amount)
            .ToList();

        var openInvoices = invoices
            .Where(i => i.BalanceAmount > 0
                && i.Status != InvoiceStatuses.Paid
                && i.Status != InvoiceStatuses.Cancelled
                && i.Status != InvoiceStatuses.Draft)
            .ToList();

        var agingBuckets = new (string Label, int Min, int Max)[]
            {
                ("Current", int.MinValue, 0),
                ("1-30 days", 1, 30),
                ("31-60 days", 31, 60),
                ("61-90 days", 61, 90),
                ("90+ days", 91, int.MaxValue)
            }
            .Select(b =>
            {
                var inBucket = openInvoices
                    .Where(i => (today.DayNumber - i.DueDate.DayNumber) is var d && d >= b.Min && d <= b.Max)
                    .ToList();
                return new AgingBucketDto
                {
                    Label = b.Label,
                    Count = inBucket.Count,
                    Amount = inBucket.Sum(i => i.BalanceAmount)
                };
            })
            .ToList();

        var recentInvoices = await _context.Invoices.AsNoTracking()
            .OrderByDescending(i => i.CreatedDate)
            .Take(5)
            .Select(i => new RecentInvoiceDto
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                CustomerName = i.Customer.CustomerName,
                GrandTotal = i.GrandTotal,
                Status = i.Status,
                InvoiceDate = i.InvoiceDate
            })
            .ToListAsync();

        var topCustomers = await _context.Customers.AsNoTracking()
            .Select(c => new TopCustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                TotalRevenue = c.Invoices.Where(i => i.Status != InvoiceStatuses.Cancelled).Sum(i => i.GrandTotal),
                InvoiceCount = c.Invoices.Count
            })
            .Where(c => c.TotalRevenue > 0)
            .OrderByDescending(c => c.TotalRevenue)
            .Take(5)
            .ToListAsync();

        var topProducts = await _context.InvoiceItems.AsNoTracking()
            .Where(ii => ii.Invoice.Status != InvoiceStatuses.Cancelled)
            .GroupBy(ii => ii.ProductName)
            .Select(g => new TopProductDto
            {
                ProductName = g.Key,
                Quantity = g.Sum(ii => ii.Quantity),
                Revenue = g.Sum(ii => ii.LineTotal)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToListAsync();

        return new DashboardDto
        {
            Stats = new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                OutstandingAmount = openInvoices.Sum(i => i.BalanceAmount),
                TotalInvoices = invoices.Count,
                TotalCustomers = customers,
                OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatuses.Overdue),
                PaidThisMonth = payments.Where(p => p.PaymentDate >= startOfMonth).Sum(p => p.AmountPaid),
                TotalCollected = totalCollected,
                CollectionRate = totalRevenue > 0 ? Math.Round(totalCollected / totalRevenue * 100, 1) : 0,
                AverageInvoiceValue = billable.Count > 0 ? Math.Round(totalRevenue / billable.Count, 2) : 0,
                RevenueThisMonth = revenueThisMonth,
                RevenueGrowthPercent = revenueLastMonth > 0
                    ? Math.Round((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100, 1)
                    : 0,
                ActiveProducts = activeProducts
            },
            MonthlyRevenue = monthlyRevenue,
            StatusBreakdown = statusBreakdown,
            RecentInvoices = recentInvoices,
            TopCustomers = topCustomers,
            PaymentMethodBreakdown = paymentMethodBreakdown,
            AgingBuckets = agingBuckets,
            TopProducts = topProducts
        };
    }
}
