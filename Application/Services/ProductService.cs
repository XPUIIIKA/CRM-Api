using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Product;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class ProductService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IProductService
{
    public async Task<ErrorOr<Guid>> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError) return guard.Errors;

        var (userId, companyId) = guard.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Error.Validation("Product.Name.Empty", "Product name is required.");

        if (request.Price < 0)
            return Error.Validation("Product.Price.Invalid", "Price cannot be negative.");

        var categoryExists = await context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.CompanyId == companyId, ct);

        if (!categoryExists)
            return Error.NotFound("Category.NotFound", "Category not found in your company.");

        var nameExists = await context.Products
            .AnyAsync(p => p.Name == request.Name && p.CompanyId == companyId, ct);

        if (nameExists)
            return Error.Conflict("Product.Name.Exists", "Product with this name already exists in your company.");

        var product = new Product(
            companyId: companyId,
            categoryId: request.CategoryId,
            name: request.Name,
            price: request.Price,
            createdBy: userId
        );

        context.Products.Add(product);
        await context.SaveChangesAsync(ct);

        return product.Id;
    }
}