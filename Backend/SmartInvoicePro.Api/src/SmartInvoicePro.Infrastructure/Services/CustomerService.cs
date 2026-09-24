using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Customer;
using SmartInvoicePro.Application.Exceptions;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IExcelExportService _excelExportService;

    public CustomerService(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IExcelExportService excelExportService)
    {
        _context = context;
        _currentUser = currentUser;
        _auditService = auditService;
        _excelExportService = excelExportService;
    }

    public async Task<PagedResult<CustomerListDto>> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.CustomerName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerListDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                CompanyName = c.CompanyName,
                Email = c.Email,
                Phone = c.Phone,
                City = c.City,
                State = c.State,
                IsActive = c.IsActive,
                InvoiceCount = c.Invoices.Count
            })
            .ToListAsync();

        return new PagedResult<CustomerListDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id)
    {
        var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == id);
        if (customer == null) throw new NotFoundException("Customer not found.");
        return Map(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = dto.CustomerName,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            TaxNumber = dto.TaxNumber,
            CreatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Customer", customer.CustomerId.ToString(), "Create", newValues: customer.CustomerName);

        return Map(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) throw new NotFoundException("Customer not found.");

        customer.CustomerName = dto.CustomerName;
        customer.CompanyName = dto.CompanyName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.City = dto.City;
        customer.State = dto.State;
        customer.Country = dto.Country;
        customer.TaxNumber = dto.TaxNumber;
        customer.IsActive = dto.IsActive;
        customer.UpdatedBy = _currentUser.UserId;
        customer.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Customer", id.ToString(), "Update");

        return Map(customer);
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _context.Customers
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null) throw new NotFoundException("Customer not found.");
        if (customer.Invoices.Any())
            throw new ValidationException("Cannot delete customer with existing invoices.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Customer", id.ToString(), "Delete");
    }

    public async Task<byte[]> ExportToExcelAsync(string? search = null)
    {
        var result = await GetAllAsync(1, 10000, search);
        return await _excelExportService.ExportCustomersAsync(result.Items);
    }

    private static CustomerDto Map(Customer c) => new()
    {
        CustomerId = c.CustomerId,
        CustomerName = c.CustomerName,
        CompanyName = c.CompanyName,
        Email = c.Email,
        Phone = c.Phone,
        Address = c.Address,
        City = c.City,
        State = c.State,
        Country = c.Country,
        TaxNumber = c.TaxNumber,
        IsActive = c.IsActive,
        CreatedDate = c.CreatedDate
    };
}
