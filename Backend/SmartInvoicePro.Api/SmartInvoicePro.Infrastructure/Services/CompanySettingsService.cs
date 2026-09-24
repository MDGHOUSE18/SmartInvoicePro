using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.CompanySettings;
using SmartInvoicePro.Application.Exceptions;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class CompanySettingsService : ICompanySettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public CompanySettingsService(ApplicationDbContext context, ICurrentUserService currentUser, IAuditService auditService)
    {
        _context = context;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<CompanySettingsDto> GetAsync()
    {
        var settings = await _context.CompanySettings.AsNoTracking().FirstOrDefaultAsync();
        if (settings == null)
            throw new NotFoundException("Company settings not found.");

        return Map(settings);
    }

    public async Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto dto)
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();
        if (settings == null)
            throw new NotFoundException("Company settings not found.");

        settings.CompanyName = dto.CompanyName;
        settings.Address = dto.Address;
        settings.City = dto.City;
        settings.State = dto.State;
        settings.Country = dto.Country;
        settings.PostalCode = dto.PostalCode;
        settings.Phone = dto.Phone;
        settings.Email = dto.Email;
        settings.Website = dto.Website;
        settings.TaxNumber = dto.TaxNumber;
        settings.LogoUrl = dto.LogoUrl;
        settings.DefaultCurrency = dto.DefaultCurrency;
        settings.TermsAndConditions = dto.TermsAndConditions;
        settings.BankName = dto.BankName;
        settings.BankAccountNumber = dto.BankAccountNumber;
        settings.BankIFSC = dto.BankIFSC;
        settings.UpdatedBy = _currentUser.UserId;
        settings.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("CompanySettings", settings.SettingId.ToString(), "Update");

        return Map(settings);
    }

    private static CompanySettingsDto Map(Domain.Entities.CompanySettings s) => new()
    {
        SettingId = s.SettingId,
        CompanyName = s.CompanyName,
        Address = s.Address,
        City = s.City,
        State = s.State,
        Country = s.Country,
        PostalCode = s.PostalCode,
        Phone = s.Phone,
        Email = s.Email,
        Website = s.Website,
        TaxNumber = s.TaxNumber,
        LogoUrl = s.LogoUrl,
        DefaultCurrency = s.DefaultCurrency,
        TermsAndConditions = s.TermsAndConditions,
        BankName = s.BankName,
        BankAccountNumber = s.BankAccountNumber,
        BankIFSC = s.BankIFSC
    };
}
