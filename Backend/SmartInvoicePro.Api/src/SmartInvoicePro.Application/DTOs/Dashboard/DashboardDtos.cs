namespace SmartInvoicePro.Application.DTOs.Dashboard;

public class DashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public IReadOnlyList<MonthlyRevenueDto> MonthlyRevenue { get; set; } = [];
    public IReadOnlyList<InvoiceStatusBreakdownDto> StatusBreakdown { get; set; } = [];
    public IReadOnlyList<RecentInvoiceDto> RecentInvoices { get; set; } = [];
    public IReadOnlyList<TopCustomerDto> TopCustomers { get; set; } = [];
    public IReadOnlyList<PaymentMethodBreakdownDto> PaymentMethodBreakdown { get; set; } = [];
    public IReadOnlyList<AgingBucketDto> AgingBuckets { get; set; } = [];
    public IReadOnlyList<TopProductDto> TopProducts { get; set; } = [];
}

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int TotalInvoices { get; set; }
    public int TotalCustomers { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal PaidThisMonth { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal AverageInvoiceValue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueGrowthPercent { get; set; }
    public int ActiveProducts { get; set; }
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Revenue { get; set; }
    public decimal Collected { get; set; }
    public int InvoiceCount { get; set; }
}

public class InvoiceStatusBreakdownDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class RecentInvoiceDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
}

public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int InvoiceCount { get; set; }
}

public class PaymentMethodBreakdownDto
{
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class AgingBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Revenue { get; set; }
}
