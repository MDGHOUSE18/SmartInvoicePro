/*
  SmartInvoice Pro - demo invoices, items, payments and notifications.

  Uses the Customers and Products that already exist in the database and generates
  ~60 invoices spread over the last 12 months (more recent months are busier, so the
  revenue trend goes up). Safe to run more than once: it does nothing if any invoice exists.

  Tax rules match InvoiceCalculationHelper: customer in Karnataka (company state) -> CGST+SGST,
  otherwise IGST, at each product's TaxPercentage. LineTotal = qty * price - discount.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM Invoices)
BEGIN
    PRINT 'Invoices already exist - nothing seeded.';
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM Customers) OR NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    PRINT 'Add customers and products first.';
    RETURN;
END;

DECLARE @CompanyState nvarchar(100) = ISNULL((SELECT TOP 1 State FROM CompanySettings), N'Karnataka');
DECLARE @AdminId uniqueidentifier = (SELECT TOP 1 UserId FROM Users WHERE Email = 'admin@acmeconsulting.in');
DECLARE @Today date = CAST(GETUTCDATE() AS date);
DECLARE @Count int = 60, @i int = 1;

CREATE TABLE #Items (ProductId uniqueidentifier, ProductName nvarchar(200), Description nvarchar(500),
                     Qty decimal(18,2), Price decimal(18,2), TaxPct decimal(5,2), Discount decimal(18,2));

BEGIN TRANSACTION;

WHILE @i <= @Count
BEGIN
    DECLARE @InvoiceId uniqueidentifier = NEWID();
    DECLARE @CustomerId uniqueidentifier, @CustomerState nvarchar(100);
    SELECT TOP 1 @CustomerId = CustomerId, @CustomerState = State FROM Customers ORDER BY NEWID();

    DECLARE @TaxType nvarchar(20) =
        CASE WHEN LTRIM(RTRIM(ISNULL(@CustomerState, @CompanyState))) = @CompanyState THEN 'CGST_SGST' ELSE 'IGST' END;

    -- Status mix: Paid 40%, PartiallyPaid 12%, Sent 18%, Overdue 15%, Draft 8%, Cancelled 7%
    DECLARE @r int = ABS(CHECKSUM(NEWID())) % 100;
    DECLARE @Status nvarchar(20) =
        CASE WHEN @r < 40 THEN 'Paid' WHEN @r < 52 THEN 'PartiallyPaid' WHEN @r < 70 THEN 'Sent'
             WHEN @r < 85 THEN 'Overdue' WHEN @r < 93 THEN 'Draft' ELSE 'Cancelled' END;

    -- Squaring a 0..1 random skews dates towards today (busier recent months).
    DECLARE @u float = (ABS(CHECKSUM(NEWID())) % 10000) / 10000.0;
    DECLARE @DaysAgo int = CAST(355 * @u * @u AS int);
    IF @Status IN ('Sent', 'Draft') SET @DaysAgo = ABS(CHECKSUM(NEWID())) % 25;
    IF @Status = 'Overdue' SET @DaysAgo = 35 + ABS(CHECKSUM(NEWID())) % 120;

    DECLARE @InvoiceDate date = DATEADD(day, -@DaysAgo, @Today);
    DECLARE @DueDate date = DATEADD(day, 30, @InvoiceDate);

    -- 1-3 line items
    TRUNCATE TABLE #Items;
    INSERT INTO #Items (ProductId, ProductName, Description, Qty, Price, TaxPct, Discount)
    SELECT TOP (1 + ABS(CHECKSUM(NEWID())) % 3)
           ProductId, ProductName, Description,
           1 + ABS(CHECKSUM(NEWID())) % 4,
           UnitPrice,
           TaxPercentage,
           CASE WHEN ABS(CHECKSUM(NEWID())) % 3 = 0 THEN 100 + ABS(CHECKSUM(NEWID())) % 400 ELSE 0 END
    FROM Products
    ORDER BY NEWID();

    DECLARE @Subtotal decimal(18,2), @Cgst decimal(18,2), @Sgst decimal(18,2), @Igst decimal(18,2);
    SELECT
        @Subtotal = SUM(Line),
        @Cgst = SUM(CASE WHEN @TaxType = 'CGST_SGST' THEN ROUND(Line * TaxPct / 2 / 100, 2) ELSE 0 END),
        @Sgst = SUM(CASE WHEN @TaxType = 'CGST_SGST' THEN ROUND(Line * TaxPct / 2 / 100, 2) ELSE 0 END),
        @Igst = SUM(CASE WHEN @TaxType = 'IGST' THEN ROUND(Line * TaxPct / 100, 2) ELSE 0 END)
    FROM (SELECT CASE WHEN Qty * Price - Discount < 0 THEN 0 ELSE Qty * Price - Discount END AS Line, TaxPct FROM #Items) x;

    DECLARE @Tax decimal(18,2) = @Cgst + @Sgst + @Igst;
    DECLARE @Grand decimal(18,2) = @Subtotal + @Tax;
    DECLARE @Paid decimal(18,2) =
        CASE @Status WHEN 'Paid' THEN @Grand
                     WHEN 'PartiallyPaid' THEN ROUND(@Grand * (20 + ABS(CHECKSUM(NEWID())) % 60) / 100.0, 2)
                     ELSE 0 END;
    DECLARE @Balance decimal(18,2) = CASE WHEN @Status = 'Cancelled' THEN 0 ELSE @Grand - @Paid END;

    INSERT INTO Invoices (InvoiceId, InvoiceNumber, InvoiceDate, DueDate, CustomerId, Status, TaxType, CurrencyCode, Notes,
                          Subtotal, CGSTAmount, SGSTAmount, IGSTAmount, TaxAmount, DiscountAmount, GrandTotal,
                          AmountPaid, BalanceAmount, CreatedBy, CreatedDate)
    VALUES (@InvoiceId, CONCAT('TMP-', @i), @InvoiceDate, @DueDate, @CustomerId, @Status, @TaxType, 'INR', NULL,
            @Subtotal, @Cgst, @Sgst, @Igst, @Tax, 0, @Grand,
            @Paid, @Balance, @AdminId, DATEADD(hour, 10, CAST(@InvoiceDate AS datetime2)));

    INSERT INTO InvoiceItems (InvoiceItemId, InvoiceId, ProductId, ProductName, Description, Quantity, UnitPrice,
                              CGSTPercentage, SGSTPercentage, IGSTPercentage, Discount, LineTotal)
    SELECT NEWID(), @InvoiceId, ProductId, ProductName, Description, Qty, Price,
           CASE WHEN @TaxType = 'CGST_SGST' THEN TaxPct / 2 ELSE 0 END,
           CASE WHEN @TaxType = 'CGST_SGST' THEN TaxPct / 2 ELSE 0 END,
           CASE WHEN @TaxType = 'IGST' THEN TaxPct ELSE 0 END,
           Discount,
           CASE WHEN Qty * Price - Discount < 0 THEN 0 ELSE Qty * Price - Discount END
    FROM #Items;

    IF @Paid > 0
    BEGIN
        DECLARE @m int = ABS(CHECKSUM(NEWID())) % 100;
        DECLARE @PayDate date = DATEADD(day, 2 + ABS(CHECKSUM(NEWID())) % 25, @InvoiceDate);
        IF @PayDate > @Today SET @PayDate = @Today;

        INSERT INTO Payments (PaymentId, InvoiceId, PaymentDate, AmountPaid, PaymentMethod, BalanceAmount,
                              ReferenceNumber, Notes, CreatedBy, CreatedDate)
        VALUES (NEWID(), @InvoiceId, @PayDate, @Paid,
                CASE WHEN @m < 45 THEN 'UPI' WHEN @m < 80 THEN 'BankTransfer' WHEN @m < 93 THEN 'CreditCard' ELSE 'Cash' END,
                @Grand - @Paid, CONCAT('TXN', 100000 + ABS(CHECKSUM(NEWID())) % 900000), NULL, @AdminId,
                DATEADD(hour, 15, CAST(@PayDate AS datetime2)));
    END;

    SET @i += 1;
END;

-- Invoice numbers in date order: INV-2026-0001, ...
WITH numbered AS (
    SELECT InvoiceNumber, InvoiceDate,
           ROW_NUMBER() OVER (ORDER BY InvoiceDate, CreatedDate, InvoiceId) AS rn
    FROM Invoices
    WHERE InvoiceNumber LIKE 'TMP-%'
)
UPDATE numbered SET InvoiceNumber = CONCAT('INV-', YEAR(InvoiceDate), '-', RIGHT(CONCAT('0000', rn), 4));

-- A few notifications for the bell icon
INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, ReferenceId, IsRead, CreatedDate)
SELECT TOP 3 NEWID(), NULL, CONCAT('Invoice ', i.InvoiceNumber, ' is overdue'),
       CONCAT(c.CustomerName, ' owes ', FORMAT(i.BalanceAmount, 'N2'), ' INR (due ', FORMAT(i.DueDate, 'dd MMM yyyy'), ')'),
       'Invoice', CAST(i.InvoiceId AS nvarchar(100)), 0, GETUTCDATE()
FROM Invoices i JOIN Customers c ON c.CustomerId = i.CustomerId
WHERE i.Status = 'Overdue'
ORDER BY i.BalanceAmount DESC;

INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, ReferenceId, IsRead, CreatedDate)
SELECT TOP 3 NEWID(), NULL, CONCAT('Payment received for ', i.InvoiceNumber),
       CONCAT(FORMAT(p.AmountPaid, 'N2'), ' INR via ', p.PaymentMethod, ' from ', c.CustomerName),
       'Payment', CAST(i.InvoiceId AS nvarchar(100)), 0, GETUTCDATE()
FROM Payments p JOIN Invoices i ON i.InvoiceId = p.InvoiceId JOIN Customers c ON c.CustomerId = i.CustomerId
ORDER BY p.PaymentDate DESC;

COMMIT TRANSACTION;
DROP TABLE #Items;

SELECT
    (SELECT COUNT(*) FROM Invoices)      AS Invoices,
    (SELECT COUNT(*) FROM InvoiceItems)  AS InvoiceItems,
    (SELECT COUNT(*) FROM Payments)      AS Payments,
    (SELECT COUNT(*) FROM Notifications) AS Notifications;
