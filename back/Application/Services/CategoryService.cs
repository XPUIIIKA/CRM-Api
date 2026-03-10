using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Category;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class CategoryService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : ICategoryService
{
    public async Task<ErrorOr<Guid>> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (userId, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Error.Validation("Category.Name.Empty", "Category name cannot be empty.");
        }

        var normalizedName = request.Name.Trim();

        var categoryNameExists = await context.Categories
            .AnyAsync(c => c.CompanyId == companyId && c.Name.ToLower() == normalizedName.ToLower(), ct);

        if (categoryNameExists)
        {
            return Error.Conflict("Category.DuplicateName", "Category with this name already exists.");
        }

        var category = new Category(
            companyId: companyId,
            name: normalizedName,
            createdBy: userId);

        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        return category.Id;
    }

    public async Task<ErrorOr<List<CategoryResponse>>> GetAllAsync(CancellationToken ct)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return categories;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Error.Validation("Category.Name.Empty", "Category name cannot be empty.");
        }

        var normalizedName = request.Name.Trim();

        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId, ct);

        if (category is null)
        {
            return Error.NotFound("Category.NotFound", "Category was not found.");
        }

        var duplicateExists = await context.Categories.AnyAsync(
            c => c.CompanyId == companyId &&
                 c.Id != id &&
                 c.Name.ToLower() == normalizedName.ToLower(),
            ct);

        if (duplicateExists)
        {
            return Error.Conflict("Category.DuplicateName", "Category with this name already exists.");
        }

        category.Rename(normalizedName);
        await context.SaveChangesAsync(ct);

        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category is null)
        {
            return Error.NotFound("Category.NotFound", "Category was not found.");
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
