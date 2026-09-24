using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartInvoicePro.Application.Validators;

namespace SmartInvoicePro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }
}
