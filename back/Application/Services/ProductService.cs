using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Product;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class ProductService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IProductService
{
    public async Task<ErrorOr<Guid>> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (userId, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Error.Validation("Product.Name.Empty", "Product name cannot be empty.");
        }

        var normalizedName = request.Name.Trim();

        if (request.Price < 0)
        {
            return Error.Validation("Product.Price.Negative", "Product price cannot be negative.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await context.Categories
                .AnyAsync(c => c.Id == request.CategoryId &&
                               (c.CompanyId == companyId || c.CompanyId == Guid.Empty), ct);

            if (!categoryExists)
            {
                return Error.NotFound("Category.NotFound", "Provided category was not found.");
            }
        }

        var product = new Product(
            companyId: companyId,
            categoryId: request.CategoryId,
            name: normalizedName,
            price: request.Price,
            createdBy: userId);

        context.Products.Add(product);
        await context.SaveChangesAsync(ct);

        return product.Id;
    }

    public async Task<ErrorOr<List<ProductResponse>>> GetAllAsync(CancellationToken ct)
    {
        var products = await context.Products
            .AsNoTracking()
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                CreatedBy = p.CreatedBy,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return products;
    }

    public async Task<ErrorOr<ProductResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await context.Products
            .AsNoTracking()
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                CreatedBy = p.CreatedBy,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", "Product was not found.");
        }

        return product;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(UpdateProductRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Error.Validation("Product.Name.Empty", "Product name cannot be empty.");
        }

        var normalizedName = request.Name.Trim();

        if (request.Price < 0)
        {
            return Error.Validation("Product.Price.Negative", "Product price cannot be negative.");
        }

        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product is null || product.CompanyId != companyId)
        {
            return Error.NotFound("Product.NotFound", "Product was not found.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await context.Categories
                .AnyAsync(c => c.Id == request.CategoryId &&
                               (c.CompanyId == companyId || c.CompanyId == Guid.Empty), ct);

            if (!categoryExists)
            {
                return Error.NotFound("Category.NotFound", "Provided category was not found.");
            }
        }

        if (!product.TryUpdateDetails(normalizedName, request.Price, request.CategoryId))
        {
            return Error.Validation("Product.Invalid", "Product data is invalid.");
        }

        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null || product.CompanyId != companyId)
        {
            return Error.NotFound("Product.NotFound", "Product was not found.");
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
