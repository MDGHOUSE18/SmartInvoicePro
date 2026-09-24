@REM SmartInvoice Pro - Run full database setup (Windows, sqlcmd required)
@echo off
set SERVER=localhost
echo Creating SmartInvoice Pro database on %SERVER%...
sqlcmd -S %SERVER% -E -i "%~dp000_SmartInvoicePro_FullSetup.sql"
echo Done. Demo users: admin@acmeconsulting.in / Admin@123
pause
