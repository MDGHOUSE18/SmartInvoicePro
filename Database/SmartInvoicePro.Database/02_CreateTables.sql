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
