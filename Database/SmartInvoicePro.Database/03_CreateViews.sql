USE SmartInvoicePro;
GO

-- =============================================
-- VIEW: Invoice Summary
-- =============================================
CREATE OR ALTER VIEW dbo.vw_InvoiceSummary
AS
SELECT
    i.InvoiceId,
    i.InvoiceNumber,
    i.InvoiceDate,
    i.DueDate,
    i.CustomerId,
    c.CustomerName,
    c.CompanyName AS CustomerCompany,
    c.Email AS CustomerEmail,
    i.Status,
    i.TaxType,
    i.CurrencyCode,
    i.Subtotal,
    i.CGSTAmount,
    i.SGSTAmount,
    i.IGSTAmount,
    i.TaxAmount,
    i.DiscountAmount,
    i.GrandTotal,
    i.AmountPaid,
    i.BalanceAmount,
    i.Notes,
    i.CreatedDate,
    i.UpdatedDate,
    CASE
        WHEN i.Status NOT IN (N'Paid', N'Cancelled') AND i.DueDate < CAST(GETUTCDATE() AS DATE) THEN 1
        ELSE 0
    END AS IsOverdue
FROM dbo.Invoices i
INNER JOIN dbo.Customers c ON c.CustomerId = i.CustomerId;
GO

-- =============================================
-- VIEW: Dashboard Stats
-- =============================================
CREATE OR ALTER VIEW dbo.vw_DashboardStats
AS
SELECT
    (SELECT COUNT(*) FROM dbo.Customers WHERE IsActive = 1) AS TotalCustomers,
    (SELECT COUNT(*) FROM dbo.Invoices) AS TotalInvoices,
    (SELECT COUNT(*) FROM dbo.Invoices WHERE Status = N'Paid') AS PaidInvoices,
    (SELECT COUNT(*) FROM dbo.Invoices WHERE Status IN (N'Sent', N'PartiallyPaid')) AS PendingInvoices,
    (SELECT COUNT(*) FROM dbo.Invoices
     WHERE Status NOT IN (N'Paid', N'Cancelled')
       AND DueDate < CAST(GETUTCDATE() AS DATE)) AS OverdueInvoices,
    (SELECT ISNULL(SUM(GrandTotal), 0) FROM dbo.Invoices WHERE Status = N'Paid') AS TotalRevenue,
    (SELECT ISNULL(SUM(GrandTotal), 0) FROM dbo.Invoices
     WHERE Status = N'Paid'
       AND YEAR(InvoiceDate) = YEAR(GETUTCDATE())
       AND MONTH(InvoiceDate) = MONTH(GETUTCDATE())) AS MonthlyRevenue;
GO

-- =============================================
-- VIEW: Customer Revenue
-- =============================================
CREATE OR ALTER VIEW dbo.vw_CustomerRevenue
AS
SELECT
    c.CustomerId,
    c.CustomerName,
    c.CompanyName,
    COUNT(i.InvoiceId) AS TotalInvoices,
    ISNULL(SUM(CASE WHEN i.Status = N'Paid' THEN i.GrandTotal ELSE 0 END), 0) AS TotalRevenue,
    ISNULL(SUM(i.BalanceAmount), 0) AS OutstandingBalance
FROM dbo.Customers c
LEFT JOIN dbo.Invoices i ON i.CustomerId = c.CustomerId
WHERE c.IsActive = 1
GROUP BY c.CustomerId, c.CustomerName, c.CompanyName;
GO

-- =============================================
-- VIEW: Monthly Revenue Trend
-- =============================================
CREATE OR ALTER VIEW dbo.vw_MonthlyRevenue
AS
SELECT
    YEAR(i.InvoiceDate) AS RevenueYear,
    MONTH(i.InvoiceDate) AS RevenueMonth,
    DATENAME(MONTH, i.InvoiceDate) + N' ' + CAST(YEAR(i.InvoiceDate) AS NVARCHAR(4)) AS MonthLabel,
    ISNULL(SUM(CASE WHEN i.Status = N'Paid' THEN i.GrandTotal ELSE 0 END), 0) AS Revenue,
    COUNT(*) AS InvoiceCount
FROM dbo.Invoices i
GROUP BY YEAR(i.InvoiceDate), MONTH(i.InvoiceDate), DATENAME(MONTH, i.InvoiceDate);
GO

-- =============================================
-- VIEW: Tax Summary
-- =============================================
CREATE OR ALTER VIEW dbo.vw_TaxSummary
AS
SELECT
    YEAR(i.InvoiceDate) AS TaxYear,
    MONTH(i.InvoiceDate) AS TaxMonth,
    i.TaxType,
    ISNULL(SUM(i.CGSTAmount), 0) AS TotalCGST,
    ISNULL(SUM(i.SGSTAmount), 0) AS TotalSGST,
    ISNULL(SUM(i.IGSTAmount), 0) AS TotalIGST,
    ISNULL(SUM(i.TaxAmount), 0) AS TotalTax
FROM dbo.Invoices i
WHERE i.Status NOT IN (N'Draft', N'Cancelled')
GROUP BY YEAR(i.InvoiceDate), MONTH(i.InvoiceDate), i.TaxType;
GO

-- =============================================
-- VIEW: Outstanding Payments
-- =============================================
CREATE OR ALTER VIEW dbo.vw_OutstandingPayments
AS
SELECT
    i.InvoiceId,
    i.InvoiceNumber,
    i.InvoiceDate,
    i.DueDate,
    c.CustomerName,
    c.Email AS CustomerEmail,
    i.GrandTotal,
    i.AmountPaid,
    i.BalanceAmount,
    i.Status,
    DATEDIFF(DAY, i.DueDate, CAST(GETUTCDATE() AS DATE)) AS DaysOverdue
FROM dbo.Invoices i
INNER JOIN dbo.Customers c ON c.CustomerId = i.CustomerId
WHERE i.BalanceAmount > 0
  AND i.Status NOT IN (N'Paid', N'Cancelled', N'Draft');
GO

PRINT 'Views created successfully.';
GO
