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
