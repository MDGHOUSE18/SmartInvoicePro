-- =====================================================================
-- SmartInvoice Pro - FULL DATABASE SETUP (all scripts in one file)
-- Target : SQL Server 2019+ / SQL Server Express
-- Run    : Open in SSMS and press F5, or
--          sqlcmd -S localhost -E -i 00_SmartInvoicePro_FullSetup.sql
-- Safe to re-run (all objects are created only if missing).
-- Logins : admin@acmeconsulting.in / Admin@123
--          staff@acmeconsulting.in / Staff@123
-- =====================================================================


-- #####################################################################
-- 01 - CREATE DATABASE   (source: 01_CreateDatabase.sql)
-- #####################################################################
-- SmartInvoice Pro - Database Creation Script
-- Target: SQL Server 2019+ / SQL Server Express
-- Run as: sqlcmd -S localhost -E -i 01_CreateDatabase.sql

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SmartInvoicePro')
BEGIN
    CREATE DATABASE SmartInvoicePro;
END
GO

USE SmartInvoicePro;
GO

-- #####################################################################
-- 02 - TABLES   (source: 02_CreateTables.sql)
-- #####################################################################
USE SmartInvoicePro;
GO

-- =============================================
-- ROLES
-- =============================================
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RoleId          INT IDENTITY(1,1) NOT NULL,
        RoleName        NVARCHAR(50) NOT NULL,
        Description     NVARCHAR(200) NULL,
        CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (RoleId),
        CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
    );
END
GO

-- =============================================
-- USERS
-- =============================================
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        Email               NVARCHAR(256) NOT NULL,
        PasswordHash        NVARCHAR(500) NOT NULL,
        FirstName           NVARCHAR(100) NOT NULL,
        LastName            NVARCHAR(100) NOT NULL,
        Phone               NVARCHAR(20) NULL,
        IsActive            BIT NOT NULL DEFAULT 1,
        RefreshToken        NVARCHAR(500) NULL,
        RefreshTokenExpiry  DATETIME2 NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate         DATETIME2 NULL,
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
    CREATE NONCLUSTERED INDEX IX_Users_Email ON dbo.Users(Email);
END
GO

-- =============================================
-- USER ROLES
-- =============================================
IF OBJECT_ID('dbo.UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles (
        UserId  UNIQUEIDENTIFIER NOT NULL,
        RoleId  INT NOT NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- PASSWORD RESET TOKENS
-- =============================================
IF OBJECT_ID('dbo.PasswordResetTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        TokenId     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        UserId      UNIQUEIDENTIFIER NOT NULL,
        Token       NVARCHAR(500) NOT NULL,
        ExpiresAt   DATETIME2 NOT NULL,
        IsUsed      BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_PasswordResetTokens PRIMARY KEY (TokenId),
        CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_Token ON dbo.PasswordResetTokens(Token);
END
GO

-- =============================================
-- COMPANY SETTINGS
-- =============================================
IF OBJECT_ID('dbo.CompanySettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CompanySettings (
        SettingId           INT IDENTITY(1,1) NOT NULL,
        CompanyName         NVARCHAR(200) NOT NULL,
        Address             NVARCHAR(500) NULL,
        City                NVARCHAR(100) NULL,
        State               NVARCHAR(100) NULL,
        Country             NVARCHAR(100) NOT NULL DEFAULT N'India',
        PostalCode          NVARCHAR(20) NULL,
        Phone               NVARCHAR(20) NULL,
        Email               NVARCHAR(256) NULL,
        Website             NVARCHAR(256) NULL,
        TaxNumber           NVARCHAR(50) NULL,
        LogoUrl             NVARCHAR(500) NULL,
        DefaultCurrency     NVARCHAR(10) NOT NULL DEFAULT N'INR',
        TermsAndConditions  NVARCHAR(MAX) NULL,
        BankName            NVARCHAR(200) NULL,
        BankAccountNumber   NVARCHAR(50) NULL,
        BankIFSC            NVARCHAR(20) NULL,
        UpdatedBy           UNIQUEIDENTIFIER NULL,
        UpdatedDate         DATETIME2 NULL,
        CONSTRAINT PK_CompanySettings PRIMARY KEY (SettingId)
    );
END
GO

-- =============================================
-- CUSTOMERS
-- =============================================
IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        CustomerId      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        CustomerName    NVARCHAR(200) NOT NULL,
        CompanyName     NVARCHAR(200) NULL,
        Email           NVARCHAR(256) NOT NULL,
        Phone           NVARCHAR(20) NULL,
        Address         NVARCHAR(500) NULL,
        City            NVARCHAR(100) NULL,
        State           NVARCHAR(100) NULL,
        Country         NVARCHAR(100) NOT NULL DEFAULT N'India',
        TaxNumber       NVARCHAR(50) NULL,
        IsActive        BIT NOT NULL DEFAULT 1,
        CreatedBy       UNIQUEIDENTIFIER NULL,
        UpdatedBy       UNIQUEIDENTIFIER NULL,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate     DATETIME2 NULL,
        CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED (CustomerId)
    );
    CREATE NONCLUSTERED INDEX IX_Customers_CustomerName ON dbo.Customers(CustomerName);
    CREATE NONCLUSTERED INDEX IX_Customers_Email ON dbo.Customers(Email);
    CREATE NONCLUSTERED INDEX IX_Customers_CompanyName ON dbo.Customers(CompanyName);
END
GO

-- =============================================
-- PRODUCTS
-- =============================================
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        ProductId       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        ProductName     NVARCHAR(200) NOT NULL,
        Description     NVARCHAR(1000) NULL,
        UnitPrice       DECIMAL(18,2) NOT NULL,
        TaxPercentage   DECIMAL(5,2) NOT NULL DEFAULT 18.00,
        CGSTPercentage  DECIMAL(5,2) NOT NULL DEFAULT 9.00,
        SGSTPercentage  DECIMAL(5,2) NOT NULL DEFAULT 9.00,
        IGSTPercentage  DECIMAL(5,2) NOT NULL DEFAULT 18.00,
        Status          NVARCHAR(20) NOT NULL DEFAULT N'Active',
        CreatedBy       UNIQUEIDENTIFIER NULL,
        UpdatedBy       UNIQUEIDENTIFIER NULL,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate     DATETIME2 NULL,
        CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (ProductId),
        CONSTRAINT CK_Products_Status CHECK (Status IN (N'Active', N'Inactive'))
    );
    CREATE NONCLUSTERED INDEX IX_Products_ProductName ON dbo.Products(ProductName);
    CREATE NONCLUSTERED INDEX IX_Products_Status ON dbo.Products(Status);
END
GO

-- =============================================
-- INVOICES
-- =============================================
IF OBJECT_ID('dbo.Invoices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Invoices (
        InvoiceId           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        InvoiceNumber       NVARCHAR(50) NOT NULL,
        InvoiceDate         DATE NOT NULL,
        DueDate             DATE NOT NULL,
        CustomerId          UNIQUEIDENTIFIER NOT NULL,
        Status              NVARCHAR(20) NOT NULL DEFAULT N'Draft',
        TaxType             NVARCHAR(20) NOT NULL DEFAULT N'CGST_SGST',
        CurrencyCode        NVARCHAR(10) NOT NULL DEFAULT N'INR',
        Notes               NVARCHAR(2000) NULL,
        Subtotal            DECIMAL(18,2) NOT NULL DEFAULT 0,
        CGSTAmount          DECIMAL(18,2) NOT NULL DEFAULT 0,
        SGSTAmount          DECIMAL(18,2) NOT NULL DEFAULT 0,
        IGSTAmount          DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxAmount           DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountAmount      DECIMAL(18,2) NOT NULL DEFAULT 0,
        GrandTotal          DECIMAL(18,2) NOT NULL DEFAULT 0,
        AmountPaid          DECIMAL(18,2) NOT NULL DEFAULT 0,
        BalanceAmount       DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreatedBy           UNIQUEIDENTIFIER NULL,
        UpdatedBy           UNIQUEIDENTIFIER NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedDate         DATETIME2 NULL,
        CONSTRAINT PK_Invoices PRIMARY KEY CLUSTERED (InvoiceId),
        CONSTRAINT UQ_Invoices_InvoiceNumber UNIQUE (InvoiceNumber),
        CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId),
        CONSTRAINT CK_Invoices_Status CHECK (Status IN (N'Draft', N'Sent', N'Paid', N'PartiallyPaid', N'Overdue', N'Cancelled')),
        CONSTRAINT CK_Invoices_TaxType CHECK (TaxType IN (N'CGST_SGST', N'IGST'))
    );
    CREATE NONCLUSTERED INDEX IX_Invoices_CustomerId ON dbo.Invoices(CustomerId);
    CREATE NONCLUSTERED INDEX IX_Invoices_Status ON dbo.Invoices(Status);
    CREATE NONCLUSTERED INDEX IX_Invoices_InvoiceDate ON dbo.Invoices(InvoiceDate);
    CREATE NONCLUSTERED INDEX IX_Invoices_DueDate ON dbo.Invoices(DueDate);
END
GO

-- =============================================
-- INVOICE ITEMS
-- =============================================
IF OBJECT_ID('dbo.InvoiceItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InvoiceItems (
        InvoiceItemId   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        InvoiceId       UNIQUEIDENTIFIER NOT NULL,
        ProductId       UNIQUEIDENTIFIER NULL,
        ProductName     NVARCHAR(200) NOT NULL,
        Description     NVARCHAR(500) NULL,
        Quantity        DECIMAL(18,2) NOT NULL DEFAULT 1,
        UnitPrice       DECIMAL(18,2) NOT NULL,
        CGSTPercentage  DECIMAL(5,2) NOT NULL DEFAULT 0,
        SGSTPercentage  DECIMAL(5,2) NOT NULL DEFAULT 0,
        IGSTPercentage  DECIMAL(5,2) NOT NULL DEFAULT 0,
        Discount        DECIMAL(18,2) NOT NULL DEFAULT 0,
        LineTotal       DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT PK_InvoiceItems PRIMARY KEY CLUSTERED (InvoiceItemId),
        CONSTRAINT FK_InvoiceItems_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId) ON DELETE CASCADE,
        CONSTRAINT FK_InvoiceItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
    );
    CREATE NONCLUSTERED INDEX IX_InvoiceItems_InvoiceId ON dbo.InvoiceItems(InvoiceId);
END
GO

-- =============================================
-- PAYMENTS
-- =============================================
IF OBJECT_ID('dbo.Payments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentId       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        InvoiceId       UNIQUEIDENTIFIER NOT NULL,
        PaymentDate     DATE NOT NULL,
        AmountPaid      DECIMAL(18,2) NOT NULL,
        PaymentMethod   NVARCHAR(30) NOT NULL,
        BalanceAmount   DECIMAL(18,2) NOT NULL DEFAULT 0,
        ReferenceNumber NVARCHAR(100) NULL,
        Notes           NVARCHAR(500) NULL,
        CreatedBy       UNIQUEIDENTIFIER NULL,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Payments PRIMARY KEY CLUSTERED (PaymentId),
        CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId),
        CONSTRAINT CK_Payments_Method CHECK (PaymentMethod IN (N'Cash', N'BankTransfer', N'UPI', N'CreditCard'))
    );
    CREATE NONCLUSTERED INDEX IX_Payments_InvoiceId ON dbo.Payments(InvoiceId);
    CREATE NONCLUSTERED INDEX IX_Payments_PaymentDate ON dbo.Payments(PaymentDate);
END
GO

-- =============================================
-- AUDIT LOGS
-- =============================================
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        AuditLogId      BIGINT IDENTITY(1,1) NOT NULL,
        EntityName      NVARCHAR(100) NOT NULL,
        EntityId        NVARCHAR(100) NOT NULL,
        Action          NVARCHAR(50) NOT NULL,
        OldValues       NVARCHAR(MAX) NULL,
        NewValues       NVARCHAR(MAX) NULL,
        UserId          UNIQUEIDENTIFIER NULL,
        UserEmail       NVARCHAR(256) NULL,
        IpAddress       NVARCHAR(50) NULL,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (AuditLogId)
    );
    CREATE NONCLUSTERED INDEX IX_AuditLogs_EntityName_EntityId ON dbo.AuditLogs(EntityName, EntityId);
    CREATE NONCLUSTERED INDEX IX_AuditLogs_CreatedDate ON dbo.AuditLogs(CreatedDate DESC);
END
GO

-- =============================================
-- NOTIFICATIONS
-- =============================================
IF OBJECT_ID('dbo.Notifications', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        NotificationId  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        UserId          UNIQUEIDENTIFIER NULL,
        Title           NVARCHAR(200) NOT NULL,
        Message         NVARCHAR(1000) NOT NULL,
        Type            NVARCHAR(50) NOT NULL,
        ReferenceId     NVARCHAR(100) NULL,
        IsRead          BIT NOT NULL DEFAULT 0,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (NotificationId),
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE SET NULL
    );
    CREATE NONCLUSTERED INDEX IX_Notifications_UserId_IsRead ON dbo.Notifications(UserId, IsRead);
END
GO

-- =============================================
-- EMAIL LOGS (Mock email integration)
-- =============================================
IF OBJECT_ID('dbo.EmailLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmailLogs (
        EmailLogId      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        ToEmail         NVARCHAR(256) NOT NULL,
        Subject         NVARCHAR(500) NOT NULL,
        Body            NVARCHAR(MAX) NOT NULL,
        AttachmentName  NVARCHAR(256) NULL,
        Status          NVARCHAR(20) NOT NULL DEFAULT N'Logged',
        SentBy          UNIQUEIDENTIFIER NULL,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmailLogs PRIMARY KEY (EmailLogId)
    );
END
GO

PRINT 'Tables created successfully.';
GO

-- #####################################################################
-- 03 - VIEWS   (source: 03_CreateViews.sql)
-- #####################################################################
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

-- #####################################################################
-- 04 - STORED PROCEDURES   (source: 04_CreateStoredProcedures.sql)
-- #####################################################################
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

-- #####################################################################
-- 05 - SEED DATA   (source: 05_SeedData.sql)
-- #####################################################################
USE SmartInvoicePro;
GO

-- =============================================
-- SEED: Roles
-- =============================================
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Admin')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES (N'Admin', N'Full system access');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Staff')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES (N'Staff', N'Invoice and customer management');
GO

-- =============================================
-- SP: Seed Demo Data
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_SeedDemoData
AS
BEGIN
    SET NOCOUNT ON;

    -- Company Settings
    IF NOT EXISTS (SELECT 1 FROM dbo.CompanySettings)
    BEGIN
        INSERT INTO dbo.CompanySettings (
            CompanyName, Address, City, State, Country, PostalCode, Phone, Email, Website,
            TaxNumber, DefaultCurrency, TermsAndConditions, BankName, BankAccountNumber, BankIFSC
        ) VALUES (
            N'Acme Consulting Pvt Ltd',
            N'42, MG Road, Brigade Towers',
            N'Bengaluru', N'Karnataka', N'India', N'560001',
            N'+91 80 4567 8900', N'billing@acmeconsulting.in', N'https://acmeconsulting.in',
            N'29AABCA1234A1Z5',
            N'INR',
            N'Payment is due within 15 days of invoice date. Late payments may attract 1.5% monthly interest. All disputes are subject to Bengaluru jurisdiction.',
            N'HDFC Bank', N'50200012345678', N'HDFC0001234'
        );
    END

    -- Demo Users — passwords are set correctly when the API runs DataSeeder on startup.
    -- If using SQL-only seed, run the API once so login works (Admin@123 / Staff@123).
    DECLARE @AdminId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
    DECLARE @StaffId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
    DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Admin');
    DECLARE @StaffRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Staff');
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@acmeconsulting.in')
    BEGIN
        INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedDate)
        VALUES (
            @AdminId,
            N'admin@acmeconsulting.in',
            N'AQAAAAIAAYagAAAAELbXpFJg5q+8xKz8vZ8mN3pQrS9tUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYzA==',
            N'Rajesh', N'Kumar', N'+91 98765 43210', 1, @Now);
        INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@AdminId, @AdminRoleId);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'staff@acmeconsulting.in')
    BEGIN
        INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedDate)
        VALUES (
            @StaffId,
            N'staff@acmeconsulting.in',
            N'AQAAAAIAAYagAAAAELbXpFJg5q+8xKz8vZ8mN3pQrS9tUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQrStUvWxYzA==',
            N'Priya', N'Sharma', N'+91 98765 43211', 1, @Now);
        INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@StaffId, @StaffRoleId);
    END

    -- Products
    IF NOT EXISTS (SELECT 1 FROM dbo.Products)
    BEGIN
        INSERT INTO dbo.Products (ProductId, ProductName, Description, UnitPrice, TaxPercentage, CGSTPercentage, SGSTPercentage, IGSTPercentage, Status, CreatedBy, CreatedDate)
        VALUES
        (NEWID(), N'Web Development', N'Custom web application development', 75000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now),
        (NEWID(), N'Mobile App Development', N'iOS and Android app development', 120000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now),
        (NEWID(), N'UI/UX Design', N'User interface and experience design', 35000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now),
        (NEWID(), N'Cloud Consulting', N'AWS/Azure cloud architecture consulting', 25000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now),
        (NEWID(), N'Maintenance & Support', N'Monthly maintenance and support package', 15000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now),
        (NEWID(), N'SEO Services', N'Search engine optimization package', 20000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now),
        (NEWID(), N'Digital Marketing', N'Social media and digital marketing', 18000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now),
        (NEWID(), N'Training Workshop', N'Corporate technology training', 45000.00, 18, 9, 9, 18, N'Active', @AdminId, @Now);
    END

    -- Customers (20 sample)
    IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
    BEGIN
        INSERT INTO dbo.Customers (CustomerId, CustomerName, CompanyName, Email, Phone, Address, City, State, Country, TaxNumber, IsActive, CreatedBy, CreatedDate)
        VALUES
        (NEWID(), N'Arun Mehta', N'TechNova Solutions', N'arun@technova.in', N'+91 98100 11111', N'101 IT Park', N'Mumbai', N'Maharashtra', N'India', N'27AABCT1234A1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Sneha Reddy', N'Reddy Enterprises', N'sneha@reddyent.com', N'+91 98480 22222', N'22 Hitech City', N'Hyderabad', N'Telangana', N'India', N'36AABCR5678B1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Vikram Singh', N'Singh & Co', N'vikram@singhco.in', N'+91 98110 33333', N'45 Connaught Place', N'New Delhi', N'Delhi', N'India', N'07AABCS9012C1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Lakshmi Iyer', N'Iyer Consulting', N'lakshmi@iyerconsulting.in', N'+91 94440 44444', N'12 Anna Salai', N'Chennai', N'Tamil Nadu', N'India', N'33AABCI3456D1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Rahul Patel', N'Patel Industries', N'rahul@patelind.com', N'+91 98250 55555', N'78 SG Highway', N'Ahmedabad', N'Gujarat', N'India', N'24AABCP7890E1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Anita Desai', N'Desai Digital', N'anita@desaidigital.in', N'+91 98220 66666', N'33 FC Road', N'Pune', N'Maharashtra', N'India', N'27AABCD2345F1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Karthik Nair', N'Nair Tech Labs', N'karthik@nairtech.in', N'+91 98470 77777', N'5 Infopark', N'Kochi', N'Kerala', N'India', N'32AABCN6789G1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Pooja Gupta', N'Gupta Retail', N'pooja@guptaretail.com', N'+91 98180 88888', N'90 Mall Road', N'Chandigarh', N'Punjab', N'India', N'04AABCG0123H1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Mohammed Ali', N'Ali Logistics', N'ali@alilogistics.in', N'+91 98460 99999', N'15 Industrial Area', N'Coimbatore', N'Tamil Nadu', N'India', N'33AABCA4567I1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Deepa Joshi', N'Joshi Media', N'deepa@joshimedia.in', N'+91 98240 10101', N'67 Residency Road', N'Bengaluru', N'Karnataka', N'India', N'29AABCD8901J1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Sanjay Verma', N'Verma Constructions', N'sanjay@vermaco.in', N'+91 98120 20202', N'23 Ring Road', N'Jaipur', N'Rajasthan', N'India', N'08AABCV2345K1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Rekha Menon', N'Menon Healthcare', N'rekha@menonhealth.in', N'+91 98450 30303', N'8 Hospital Road', N'Thiruvananthapuram', N'Kerala', N'India', N'32AABCM6789L1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Amit Shah', N'Shah Finance', N'amit@shahfinance.com', N'+91 98230 40404', N'44 BKC', N'Mumbai', N'Maharashtra', N'India', N'27AABCS0123M1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Neha Kapoor', N'Kapoor Fashion', N'neha@kapoorfashion.in', N'+91 98150 50505', N'12 Fashion Street', N'Delhi', N'Delhi', N'India', N'07AABCK4567N1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Ravi Krishnan', N'Krishnan Foods', N'ravi@krishnanfoods.in', N'+91 98430 60606', N'56 Market Road', N'Madurai', N'Tamil Nadu', N'India', N'33AABCK8901O1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Sunita Rao', N'Rao Education', N'sunita@raoeducation.in', N'+91 98260 70707', N'3 University Road', N'Mysuru', N'Karnataka', N'India', N'29AABCR2345P1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Gaurav Malhotra', N'Malhotra Exports', N'gaurav@malhotraexports.com', N'+91 98130 80808', N'77 Export Zone', N'Noida', N'Uttar Pradesh', N'India', N'09AABCM6789Q1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Kavita Bansal', N'Bansal Interiors', N'kavita@bansalinteriors.in', N'+91 98420 90909', N'19 Design Hub', N'Gurgaon', N'Haryana', N'India', N'06AABCB0123R1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Harish Choudhary', N'Choudhary Farms', N'harish@choudharyfarms.in', N'+91 98210 11011', N'100 Rural Road', N'Nagpur', N'Maharashtra', N'India', N'27AABCC4567S1Z5', 1, @AdminId, @Now),
        (NEWID(), N'Meera Das', N'Das Creative Agency', N'meera@dascreative.in', N'+91 98410 12012', N'28 Creative Lane', N'Kolkata', N'West Bengal', N'India', N'19AABCD8901T1Z5', 1, @AdminId, @Now);
    END
END
GO

-- Run initial seed
EXEC dbo.sp_SeedDemoData;
GO

PRINT 'Seed data applied successfully.';
PRINT 'Demo credentials: admin@acmeconsulting.in / Admin@123 | staff@acmeconsulting.in / Staff@123';
GO

-- #####################################################################
-- 06 - DEMO USERS (working passwords)   (source: 07_RecreateDemoUsers.sql)
-- #####################################################################
USE SmartInvoicePro;
GO

-- Recreate demo Admin and Staff users with working passwords
-- Admin@123 / Staff@123

DECLARE @AdminId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @StaffId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Admin');
DECLARE @StaffRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Staff');
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF @AdminRoleId IS NULL OR @StaffRoleId IS NULL
BEGIN
    RAISERROR('Roles not found. Run 05_SeedData.sql first to create Admin and Staff roles.', 16, 1);
    RETURN;
END

-- Admin user
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@acmeconsulting.in')
BEGIN
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedDate)
    VALUES (
        @AdminId,
        N'admin@acmeconsulting.in',
        N'AQAAAAEAACcQAAAAEM1555RHa53BZ8m8gnSmqwCxxeAUQcFENlpj90Hg8/e5CUFPt7KPaC0UPegbRm2qCw==',
        N'Rajesh', N'Kumar', N'+91 98765 43210', 1, @Now);

    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@AdminId, @AdminRoleId);
    PRINT 'Admin user created.';
END
ELSE
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'AQAAAAEAACcQAAAAEM1555RHa53BZ8m8gnSmqwCxxeAUQcFENlpj90Hg8/e5CUFPt7KPaC0UPegbRm2qCw==',
        IsActive = 1, FirstName = N'Rajesh', LastName = N'Kumar', UpdatedDate = @Now
    WHERE Email = N'admin@acmeconsulting.in';

    IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles ur
                   INNER JOIN dbo.Users u ON u.UserId = ur.UserId
                   WHERE u.Email = N'admin@acmeconsulting.in' AND ur.RoleId = @AdminRoleId)
        INSERT INTO dbo.UserRoles (UserId, RoleId)
        SELECT UserId, @AdminRoleId FROM dbo.Users WHERE Email = N'admin@acmeconsulting.in';

    PRINT 'Admin user updated.';
END

-- Staff user
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'staff@acmeconsulting.in')
BEGIN
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedDate)
    VALUES (
        @StaffId,
        N'staff@acmeconsulting.in',
        N'AQAAAAEAACcQAAAAEBuJuOqwCpsuhlTEHpUIT3drvW9/51E8Jwwu9Vf/Kn2TzMPA7JK1omASrl1uZTXlJg==',
        N'Priya', N'Sharma', N'+91 98765 43211', 1, @Now);

    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@StaffId, @StaffRoleId);
    PRINT 'Staff user created.';
END
ELSE
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'AQAAAAEAACcQAAAAEBuJuOqwCpsuhlTEHpUIT3drvW9/51E8Jwwu9Vf/Kn2TzMPA7JK1omASrl1uZTXlJg==',
        IsActive = 1, FirstName = N'Priya', LastName = N'Sharma', UpdatedDate = @Now
    WHERE Email = N'staff@acmeconsulting.in';

    IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles ur
                   INNER JOIN dbo.Users u ON u.UserId = ur.UserId
                   WHERE u.Email = N'staff@acmeconsulting.in' AND ur.RoleId = @StaffRoleId)
        INSERT INTO dbo.UserRoles (UserId, RoleId)
        SELECT UserId, @StaffRoleId FROM dbo.Users WHERE Email = N'staff@acmeconsulting.in';

    PRINT 'Staff user updated.';
END

-- Verify
SELECT u.Email, r.RoleName, u.IsActive, LEFT(u.PasswordHash, 20) AS HashPrefix
FROM dbo.Users u
LEFT JOIN dbo.UserRoles ur ON ur.UserId = u.UserId
LEFT JOIN dbo.Roles r ON r.RoleId = ur.RoleId
WHERE u.Email IN (N'admin@acmeconsulting.in', N'staff@acmeconsulting.in');

PRINT 'Done. Login: admin@acmeconsulting.in / Admin@123 | staff@acmeconsulting.in / Staff@123';
GO

PRINT '=== SmartInvoice Pro full setup completed ===';
GO
