USE SmartInvoicePro;
GO

-- Fix demo login passwords (ASP.NET Identity PBKDF2 hashes for Admin@123 / Staff@123)
-- Run this if login fails after SQL seed and API has not updated hashes yet.

UPDATE dbo.Users
SET PasswordHash = N'AQAAAAEAACcQAAAAEM1555RHa53BZ8m8gnSmqwCxxeAUQcFENlpj90Hg8/e5CUFPt7KPaC0UPegbRm2qCw==',
    IsActive = 1,
    UpdatedDate = SYSUTCDATETIME()
WHERE Email = N'admin@acmeconsulting.in';

UPDATE dbo.Users
SET PasswordHash = N'AQAAAAEAACcQAAAAEBuJuOqwCpsuhlTEHpUIT3drvW9/51E8Jwwu9Vf/Kn2TzMPA7JK1omASrl1uZTXlJg==',
    IsActive = 1,
    UpdatedDate = SYSUTCDATETIME()
WHERE Email = N'staff@acmeconsulting.in';

PRINT 'Demo passwords fixed. Login: admin@acmeconsulting.in / Admin@123';
GO
