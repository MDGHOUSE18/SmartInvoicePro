USE SmartInvoicePro;
GO

-- =============================================
-- SP: Get Dashboard Statistics
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_DashboardStats;
END
GO

-- =============================================
-- SP: Get Revenue Trend (last N months)
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetRevenueTrend
    @Months INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Months)
        RevenueYear,
        RevenueMonth,
        MonthLabel,
        Revenue,
        InvoiceCount
    FROM dbo.vw_MonthlyRevenue
    ORDER BY RevenueYear DESC, RevenueMonth DESC;
END
GO

-- =============================================
-- SP: Get Invoice Status Breakdown
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetInvoiceStatusBreakdown
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Status,
        COUNT(*) AS InvoiceCount,
        ISNULL(SUM(GrandTotal), 0) AS TotalAmount
    FROM dbo.Invoices
    GROUP BY Status
    ORDER BY InvoiceCount DESC;
END
GO

-- =============================================
-- SP: Get Top Customers
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetTopCustomers
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN)
        CustomerId,
        CustomerName,
        CompanyName,
        TotalInvoices,
        TotalRevenue,
        OutstandingBalance
    FROM dbo.vw_CustomerRevenue
    ORDER BY TotalRevenue DESC;
END
GO

-- =============================================
-- SP: Search Customers (paginated)
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_SearchCustomers
    @SearchTerm NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = N'CustomerName',
    @SortDirection NVARCHAR(4) = N'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH Filtered AS (
        SELECT *
        FROM dbo.Customers
        WHERE IsActive = 1
          AND (@SearchTerm IS NULL OR @SearchTerm = N''
               OR CustomerName LIKE N'%' + @SearchTerm + N'%'
               OR CompanyName LIKE N'%' + @SearchTerm + N'%'
               OR Email LIKE N'%' + @SearchTerm + N'%'
               OR Phone LIKE N'%' + @SearchTerm + N'%')
    )
    SELECT COUNT(*) AS TotalCount FROM Filtered;

    SELECT *
    FROM Filtered
    ORDER BY
        CASE WHEN @SortColumn = N'CustomerName' AND @SortDirection = N'ASC' THEN CustomerName END ASC,
        CASE WHEN @SortColumn = N'CustomerName' AND @SortDirection = N'DESC' THEN CustomerName END DESC,
        CASE WHEN @SortColumn = N'CreatedDate' AND @SortDirection = N'ASC' THEN CreatedDate END ASC,
        CASE WHEN @SortColumn = N'CreatedDate' AND @SortDirection = N'DESC' THEN CreatedDate END DESC,
        CustomerName ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: Generate Next Invoice Number
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GenerateInvoiceNumber
    @InvoiceNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Year NVARCHAR(4) = CAST(YEAR(GETUTCDATE()) AS NVARCHAR(4));
    DECLARE @Prefix NVARCHAR(20) = N'INV-' + @Year + N'-';
    DECLARE @NextSeq INT;

    SELECT @NextSeq = ISNULL(MAX(CAST(RIGHT(InvoiceNumber, 5) AS INT)), 0) + 1
    FROM dbo.Invoices
    WHERE InvoiceNumber LIKE @Prefix + N'%';

    SET @InvoiceNumber = @Prefix + RIGHT(N'00000' + CAST(@NextSeq AS NVARCHAR(5)), 5);
END
GO

-- =============================================
-- SP: Update Invoice Totals
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_UpdateInvoiceTotals
    @InvoiceId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TaxType NVARCHAR(20);
    SELECT @TaxType = TaxType FROM dbo.Invoices WHERE InvoiceId = @InvoiceId;

    DECLARE @Subtotal DECIMAL(18,2);
    DECLARE @CGST DECIMAL(18,2);
    DECLARE @SGST DECIMAL(18,2);
    DECLARE @IGST DECIMAL(18,2);
    DECLARE @Discount DECIMAL(18,2);

    SELECT
        @Subtotal = ISNULL(SUM((Quantity * UnitPrice) - Discount), 0),
        @Discount = ISNULL(SUM(Discount), 0),
        @CGST = ISNULL(SUM(((Quantity * UnitPrice) - Discount) * CGSTPercentage / 100), 0),
        @SGST = ISNULL(SUM(((Quantity * UnitPrice) - Discount) * SGSTPercentage / 100), 0),
        @IGST = ISNULL(SUM(((Quantity * UnitPrice) - Discount) * IGSTPercentage / 100), 0)
    FROM dbo.InvoiceItems
    WHERE InvoiceId = @InvoiceId;

    UPDATE dbo.Invoices
    SET
        Subtotal = @Subtotal,
        CGSTAmount = CASE WHEN @TaxType = N'CGST_SGST' THEN @CGST ELSE 0 END,
        SGSTAmount = CASE WHEN @TaxType = N'CGST_SGST' THEN @SGST ELSE 0 END,
        IGSTAmount = CASE WHEN @TaxType = N'IGST' THEN @IGST ELSE 0 END,
        TaxAmount = CASE WHEN @TaxType = N'CGST_SGST' THEN @CGST + @SGST ELSE @IGST END,
        DiscountAmount = @Discount,
        GrandTotal = @Subtotal + CASE WHEN @TaxType = N'CGST_SGST' THEN @CGST + @SGST ELSE @IGST END,
        BalanceAmount = (@Subtotal + CASE WHEN @TaxType = N'CGST_SGST' THEN @CGST + @SGST ELSE @IGST END) - AmountPaid,
        UpdatedDate = SYSUTCDATETIME()
    WHERE InvoiceId = @InvoiceId;
END
GO

-- =============================================
-- SP: Record Payment
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_RecordPayment
    @InvoiceId UNIQUEIDENTIFIER,
    @PaymentDate DATE,
    @AmountPaid DECIMAL(18,2),
    @PaymentMethod NVARCHAR(30),
    @ReferenceNumber NVARCHAR(100) = NULL,
    @Notes NVARCHAR(500) = NULL,
    @CreatedBy UNIQUEIDENTIFIER = NULL,
    @PaymentId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    SET @PaymentId = NEWID();

    DECLARE @CurrentPaid DECIMAL(18,2);
    DECLARE @GrandTotal DECIMAL(18,2);

    SELECT @CurrentPaid = AmountPaid, @GrandTotal = GrandTotal
    FROM dbo.Invoices WHERE InvoiceId = @InvoiceId;

    DECLARE @NewPaid DECIMAL(18,2) = @CurrentPaid + @AmountPaid;
    DECLARE @Balance DECIMAL(18,2) = @GrandTotal - @NewPaid;
    DECLARE @Status NVARCHAR(20);

    SET @Status = CASE
        WHEN @Balance <= 0 THEN N'Paid'
        WHEN @NewPaid > 0 THEN N'PartiallyPaid'
        ELSE (SELECT Status FROM dbo.Invoices WHERE InvoiceId = @InvoiceId)
    END;

    INSERT INTO dbo.Payments (PaymentId, InvoiceId, PaymentDate, AmountPaid, PaymentMethod, BalanceAmount, ReferenceNumber, Notes, CreatedBy)
    VALUES (@PaymentId, @InvoiceId, @PaymentDate, @AmountPaid, @PaymentMethod, @Balance, @ReferenceNumber, @Notes, @CreatedBy);

    UPDATE dbo.Invoices
    SET AmountPaid = @NewPaid,
        BalanceAmount = CASE WHEN @Balance < 0 THEN 0 ELSE @Balance END,
        Status = @Status,
        UpdatedDate = SYSUTCDATETIME(),
        UpdatedBy = @CreatedBy
    WHERE InvoiceId = @InvoiceId;

    COMMIT TRANSACTION;
END
GO

-- =============================================
-- SP: Get Tax Summary Report
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetTaxSummary
    @Year INT = NULL,
    @Month INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @Year = ISNULL(@Year, YEAR(GETUTCDATE()));

    SELECT
        TaxYear,
        TaxMonth,
        TaxType,
        TotalCGST,
        TotalSGST,
        TotalIGST,
        TotalTax
    FROM dbo.vw_TaxSummary
    WHERE TaxYear = @Year
      AND (@Month IS NULL OR TaxMonth = @Month)
    ORDER BY TaxMonth, TaxType;
END
GO

-- =============================================
-- SP: Reset Demo Data
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_ResetDemoData
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    DELETE FROM dbo.Notifications;
    DELETE FROM dbo.EmailLogs;
    DELETE FROM dbo.AuditLogs;
    DELETE FROM dbo.Payments;
    DELETE FROM dbo.InvoiceItems;
    DELETE FROM dbo.Invoices;
    DELETE FROM dbo.Products;
    DELETE FROM dbo.Customers;
    DELETE FROM dbo.PasswordResetTokens;
    DELETE FROM dbo.UserRoles;
    DELETE FROM dbo.Users;
    DELETE FROM dbo.CompanySettings;

    COMMIT TRANSACTION;

    EXEC dbo.sp_SeedDemoData;
END
GO

PRINT 'Stored procedures created successfully.';
GO
