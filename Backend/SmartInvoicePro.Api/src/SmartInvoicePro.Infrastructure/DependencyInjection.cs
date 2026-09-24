using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Data;
using SmartInvoicePro.Infrastructure.Repositories;
using SmartInvoicePro.Infrastructure.Security;
using SmartInvoicePro.Infrastructure.Services;

namespace SmartInvoicePro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpContextAccessor();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<PasswordHasherService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ICompanySettingsService, CompanySettingsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
