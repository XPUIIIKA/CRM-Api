using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
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
    public async Task<ErrorOr<CreateCompanyResult>> CreateAsync(CreateCompanyRequest request, CancellationToken ct)
    {
        if (userContext.UserId is null || !userContext.IsSystemAdmin)
            return Error.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            return Error.Validation("Company.Name.Empty");

        if (string.IsNullOrWhiteSpace(request.OwnerEmail))
            return Error.Validation("Owner.Email.Empty");

        var emailExists = await context.Users
            .AnyAsync(x => x.Email == request.OwnerEmail, ct);

        if (emailExists)
            return Error.Conflict(
                code: "User.Email.Exists",
                description: "User with this email already exists"
            );

        var company = new Company(
            name: request.CompanyName,
            createdBy: userContext.UserId.Value
        );

        var plainPassword = passwordGenerator.Generate();
        var passwordHash = passwordHasher.Hash(plainPassword);

        var owner = new User(
            companyId: company.Id,
            roleId: RoleIds.CompanyOwner,
            fullName: request.OwnerFullName,
            email: request.OwnerEmail,
            passwordHash: passwordHash,
            createdBy: userContext.UserId.Value
        );

        context.Companies.Add(company);
        context.Users.Add(owner);

        await context.SaveChangesAsync(ct);

        return new CreateCompanyResult
        {
            CompanyId = company.Id,
            OwnerUserId = owner.Id,
            GeneratedPassword = plainPassword
        };
    }

    public async Task<ErrorOr<bool>> ExistsAsync(Guid companyId, CancellationToken ct)
    {
        if (userContext.UserId is null)
            return Error.Unauthorized();

        var exists = await context.Companies
            .AnyAsync(x => x.Id == companyId, ct);

        if (!exists)
            return Error.NotFound(
                code: "Company.NotFound",
                description: $"Company with ID {companyId} does not exist"
            );

        return exists;
    }
}