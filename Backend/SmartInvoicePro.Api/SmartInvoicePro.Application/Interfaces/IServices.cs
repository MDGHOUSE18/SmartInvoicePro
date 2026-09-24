using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.AuditLog;
using SmartInvoicePro.Application.DTOs.Auth;
using SmartInvoicePro.Application.DTOs.CompanySettings;
using SmartInvoicePro.Application.DTOs.Customer;
using SmartInvoicePro.Application.DTOs.Dashboard;
using SmartInvoicePro.Application.DTOs.Invoice;
using SmartInvoicePro.Application.DTOs.Notification;
using SmartInvoicePro.Application.DTOs.Payment;
using SmartInvoicePro.Application.DTOs.Product;
using SmartInvoicePro.Application.DTOs.Reports;

namespace SmartInvoicePro.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<UserDto> GetCurrentUserAsync();
    Task<string?> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
    Task ChangePasswordAsync(ChangePasswordRequestDto request);
}

public interface ICustomerService
{
    Task<PagedResult<CustomerListDto>> GetAllAsync(int page, int pageSize, string? search = null);
    Task<CustomerDto> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto dto);
    Task DeleteAsync(Guid id);
    Task<byte[]> ExportToExcelAsync(string? search = null);
}

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? search = null);
    Task<ProductDto> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto);
    Task DeleteAsync(Guid id);
}

public interface IInvoiceService
{
    Task<PagedResult<InvoiceListDto>> GetAllAsync(int page, int pageSize, string? status = null, Guid? customerId = null, string? search = null);
    Task<InvoiceDto> GetByIdAsync(Guid id);
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto);
    Task<InvoiceDto> UpdateAsync(Guid id, UpdateInvoiceDto dto);
    Task DeleteAsync(Guid id);
    Task<InvoiceDto> CloneAsync(Guid id);
    Task<InvoiceDto> MarkAsPaidAsync(Guid id);
    Task<byte[]> GeneratePdfAsync(Guid id);
    Task SendInvoiceEmailAsync(Guid id);
    Task<InvoiceSummaryDto> GetSummaryAsync();
}

public interface IPaymentService
{
    Task<PagedResult<PaymentDto>> GetAllAsync(int page, int pageSize, Guid? invoiceId = null);
    Task<PaymentDto> GetByIdAsync(Guid id);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto);
    Task DeleteAsync(Guid id);
}

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(ReportFilterDto filter);
    Task<TaxReportDto> GetTaxReportAsync(ReportFilterDto filter);
    Task<IReadOnlyList<CustomerReportDto>> GetCustomerReportAsync(ReportFilterDto filter);
    Task<PaymentReportDto> GetPaymentReportAsync(ReportFilterDto filter);
    Task<byte[]> ExportSalesReportExcelAsync(ReportFilterDto filter);
    Task<byte[]> ExportTaxReportExcelAsync(ReportFilterDto filter);
}

public interface ICompanySettingsService
{
    Task<CompanySettingsDto> GetAsync();
    Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto dto);
}

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetForCurrentUserAsync(int page, int pageSize, bool? unreadOnly = null);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync();
    Task<int> GetUnreadCountAsync();
}

public interface IAuditService
{
    Task<PagedResult<AuditLogDto>> GetAllAsync(AuditLogFilterDto filter);
    Task LogAsync(string entityName, string entityId, string action, string? oldValues = null, string? newValues = null);
}

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, string? attachmentName = null, byte[]? attachment = null);
}

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice, CompanySettingsDto company);
}

public interface IExcelExportService
{
    Task<byte[]> ExportSalesReportAsync(SalesReportDto report);
    Task<byte[]> ExportTaxReportAsync(TaxReportDto report);
    Task<byte[]> ExportCustomersAsync(IReadOnlyList<CustomerListDto> customers);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? IpAddress { get; }
    bool IsInRole(string role);
}

public interface IDataSeeder
{
    Task SeedAsync();
    Task ResetDemoDataAsync();
    Task EnsureDemoCredentialsAsync();
}
