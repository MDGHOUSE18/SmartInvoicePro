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
