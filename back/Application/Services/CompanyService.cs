using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs._General;
using Application.DTOs.Company.CreateCompany;
using Domain.Constants;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class CompanyService(
    ICrmDbContext context,
    ICurrentUserContext userContext,
    IPasswordGenerator passwordGenerator,
    IHasher passwordHasher) : ICompanyService
{
    public async Task<ErrorOr<CreateCompanyResponse>> CreateAsync(CreateCompanyRequest request, CancellationToken ct)
    {
        if (!userContext.IsSystemAdmin || userContext.UserId is null)
        {
            return Error.Unauthorized("Auth.Unauthorized", "Only system administrators can create companies.");
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return Error.Validation("Company.Name.Empty", "Company name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.OwnerEmail))
        {
            return Error.Validation("User.Email.Empty", "Owner email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.OwnerFullName))
        {
            return Error.Validation("User.FullName.Empty", "Owner full name cannot be empty.");
        }

        var normalizedOwnerEmail = request.OwnerEmail.Trim();

        var emailExists = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Email == normalizedOwnerEmail, ct);

        var adminEmailExists = await context.SystemAdmins
            .AnyAsync(x => x.Email == normalizedOwnerEmail, ct);

        if (emailExists || adminEmailExists)
        {
            return Error.Conflict("User.Email.Exists", "User with this email already exists.");
        }

        var company = new Company(
            name: request.CompanyName.Trim(),
            createdBy: userContext.UserId.Value);

        var plainPassword = passwordGenerator.Generate();
        var owner = new User(
            roleId: RoleIds.CompanyOwner,
            fullName: request.OwnerFullName.Trim(),
            email: normalizedOwnerEmail,
            passwordHash: passwordHasher.Hash(plainPassword),
            createdBy: userContext.UserId.Value,
            companyId: company.Id);

        context.Companies.Add(company);
        context.Users.Add(owner);

        await context.SaveChangesAsync(ct);

        return new CreateCompanyResponse
        {
            CompanyId = company.Id,
            OwnerId = owner.Id,
            GeneratedPassword = plainPassword
        };
    }

    public async Task<ErrorOr<CompanyResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        if (!userContext.IsSystemAdmin)
        {
            if (userContext.CompanyId is null)
            {
                return Error.Unauthorized("Auth.Unauthorized", "Company context is required.");
            }

            if (id != userContext.CompanyId.Value)
            {
                return Error.Forbidden("Company.Forbidden", "Access to this company is forbidden.");
            }
        }

        var company = await context.Companies
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (company is null)
        {
            return Error.NotFound("Company.NotFound", "Company was not found.");
        }

        var ownerId = await context.Users
            .IgnoreQueryFilters()
            .Where(u => u.CompanyId == id && u.RoleId == RoleIds.CompanyOwner)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(ct);

        return new CompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            CreatedAt = company.CreatedAt,
            OwnerId = ownerId
        };
    }

    public async Task<ErrorOr<PagedList<CompanyResponse>>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        if (!userContext.IsSystemAdmin)
        {
            return Error.Unauthorized("Auth.Unauthorized", "Only system administrators can list companies.");
        }

        if (page <= 0 || pageSize <= 0)
        {
            return Error.Validation("Pagination.Invalid", "Page and page size must be greater than zero.");
        }

        var companiesQuery = context.Companies
            .IgnoreQueryFilters()
            .AsNoTracking();

        var totalCount = await companiesQuery.CountAsync(ct);

        var ownersQuery = context.Users
            .IgnoreQueryFilters()
            .Where(u => u.RoleId == RoleIds.CompanyOwner)
            .GroupBy(u => u.CompanyId)
            .Select(g => new
            {
                CompanyId = g.Key,
                OwnerId = g.Select(x => (Guid?)x.Id).FirstOrDefault()
            });

        var items = await (
                from company in companiesQuery
                join owner in ownersQuery on company.Id equals owner.CompanyId into companyOwners
                from owner in companyOwners.DefaultIfEmpty()
                orderby company.CreatedAt descending
                select new CompanyResponse
                {
                    Id = company.Id,
                    Name = company.Name,
                    CreatedAt = company.CreatedAt,
                    OwnerId = owner != null ? owner.OwnerId : null
                })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<CompanyResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ErrorOr<Deleted>> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct)
    {
        if (!userContext.IsSystemAdmin)
        {
            return Error.Unauthorized("Auth.Unauthorized", "Only system administrators can update company status.");
        }

        var company = await context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (company is null)
        {
            return Error.NotFound("Company.NotFound", "Company was not found.");
        }

        company.UpdateStatus(isActive);
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }

    public async Task<ErrorOr<bool>> ExistsAsync(Guid companyId, CancellationToken ct)
    {
        var exists = await context.Companies
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == companyId, ct);

        return exists;
    }
}
