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
