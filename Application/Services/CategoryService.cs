using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class CategoryService(ICrmDbContext context, ICurrentUserContext userContext) : ICategoryService
{

    public async Task<ErrorOr<Guid>> CreateAsync(
        string name,
        CancellationToken ct
    )
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        
        if (guard.IsError)
            return guard.Errors;
        
        var (userId, companyId) = guard.Value;
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation(
                code: "Category.Name.Empty",
                description: "Category name cannot be empty"
            );
        }

        var companyExists = await context.Companies
            .AnyAsync(x => x.Id == companyId, ct);

        if (!companyExists)
        {
            return Error.NotFound(
                code: "Company.NotFound",
                description: "Company not found"
            );
        }
        
        name = name.Trim();
        
        var categoryExists = await context.Categories
            .AnyAsync(
                x => x.CompanyId == companyId
                     && x.Name == name,
                ct
            );

        if (categoryExists)
        {
            return Error.Conflict(
                code: "Category.AlreadyExists",
                description: "Category with the same name already exists"
            );
        }
        
        var category = new Category(
            companyId: companyId,
            name: name,
            createdBy: userId
        );

        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        return category.Id;
    }
}
