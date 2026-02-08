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
            return Error.Unauthorized();

        var emailExists = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Email == request.OwnerEmail, ct);

        if (emailExists)
            return Error.Conflict("User.Email.Exists", "User with this email already exists");

        var company = new Company(
            name: request.CompanyName,
            createdBy: userContext.UserId.Value
        );

        var plainPassword = passwordGenerator.Generate();
        var owner = new User(
            companyId: company.Id,
            roleId: RoleIds.CompanyOwner,
            fullName: request.OwnerFullName,
            email: request.OwnerEmail,
            passwordHash: passwordHasher.Hash(plainPassword),
            createdBy: userContext.UserId.Value
        );

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
        var query = userContext.IsSystemAdmin 
            ? context.Companies.IgnoreQueryFilters() 
            : context.Companies;

        var company = await query
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (company is null) return Error.NotFound("Company.NotFound");

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
        if (!userContext.IsSystemAdmin) return Error.Unauthorized();

        var companiesQuery = context.Companies.IgnoreQueryFilters().AsNoTracking();

        var query = from c in companiesQuery
            join u in context.Users.IgnoreQueryFilters() 
                on c.Id equals u.CompanyId into owners
            from owner in owners.DefaultIfEmpty()
            select new CompanyResponse
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                OwnerId = owner != null ? owner.Id : Guid.Empty 
            };

        var totalCount = await companiesQuery.CountAsync(ct);
    
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
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
        if (!userContext.IsSystemAdmin) return Error.Unauthorized();

        var company = await context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (company is null) return Error.NotFound("Company.NotFound");

        await context.SaveChangesAsync(ct);
        return Result.Deleted;
    }

    public async Task<ErrorOr<bool>> ExistsAsync(Guid companyId, CancellationToken ct)
    {
        return await context.Companies
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == companyId, ct);
    }
}