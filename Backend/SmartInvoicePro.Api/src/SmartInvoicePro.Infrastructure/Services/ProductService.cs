using Microsoft.EntityFrameworkCore;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.DTOs.Product;
using SmartInvoicePro.Application.Exceptions;
using SmartInvoicePro.Application.Interfaces;
using SmartInvoicePro.Domain.Entities;
using SmartInvoicePro.Infrastructure.Data;

namespace SmartInvoicePro.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public ProductService(ApplicationDbContext context, ICurrentUserService currentUser, IAuditService auditService)
    {
        _context = context;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = products.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) throw new NotFoundException("Product not found.");
        return Map(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var half = dto.TaxPercentage / 2m;
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            ProductName = dto.ProductName,
            Description = dto.Description,
            UnitPrice = dto.UnitPrice,
            TaxPercentage = dto.TaxPercentage,
            CGSTPercentage = half,
            SGSTPercentage = half,
            IGSTPercentage = dto.TaxPercentage,
            Status = dto.Status,
            CreatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Product", product.ProductId.ToString(), "Create");

        return Map(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) throw new NotFoundException("Product not found.");

        var half = dto.TaxPercentage / 2m;
        product.ProductName = dto.ProductName;
        product.Description = dto.Description;
        product.UnitPrice = dto.UnitPrice;
        product.TaxPercentage = dto.TaxPercentage;
        product.CGSTPercentage = half;
        product.SGSTPercentage = half;
        product.IGSTPercentage = dto.TaxPercentage;
        product.Status = dto.Status;
        product.UpdatedBy = _currentUser.UserId;
        product.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Product", id.ToString(), "Update");

        return Map(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) throw new NotFoundException("Product not found.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Product", id.ToString(), "Delete");
    }

    private static ProductDto Map(Product p) => new()
    {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        Description = p.Description,
        UnitPrice = p.UnitPrice,
        TaxPercentage = p.TaxPercentage,
        CGSTPercentage = p.CGSTPercentage,
        SGSTPercentage = p.SGSTPercentage,
        IGSTPercentage = p.IGSTPercentage,
        Status = p.Status,
        CreatedDate = p.CreatedDate
    };
}
