using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.SystemAdmin;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class SystemAdminService(
    ICrmDbContext context,
    IHasher passwordHasher,
    ICurrentUserContext userContext) : ISystemAdminService
{
    public async Task<ErrorOr<Guid>> CreateAsync(CreateSystemAdminRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Login))
        {
            return Error.Validation("Admin.InvalidInput", "Email and login are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Error.Validation("Admin.Password.Empty", "Password is required.");
        }

        var normalizedEmail = request.Email.Trim();
        var normalizedLogin = request.Login.Trim();

        var anyAdminExists = await context.SystemAdmins.AnyAsync(ct);

        if (anyAdminExists)
        {
            var currentAdmin = await context.SystemAdmins
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == userContext.UserId, ct);

            if (currentAdmin is null || !currentAdmin.IsRoot)
            {
                return Error.Forbidden("Auth.Forbidden", "Only root system administrator can create other admins.");
            }
        }

        var duplicateAdminExists = await context.SystemAdmins
            .AnyAsync(a => a.Email == normalizedEmail || a.Login == normalizedLogin, ct);

        var duplicateUserEmailExists = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == normalizedEmail, ct);

        if (duplicateAdminExists || duplicateUserEmailExists)
        {
            return Error.Conflict("Admin.Duplicate", "Administrator with this email or login already exists.");
        }

        var admin = new SystemAdmin(
            email: normalizedEmail,
            login: normalizedLogin,
            passwordHash: passwordHasher.Hash(request.Password),
            isRoot: request.IsRoot);

        context.SystemAdmins.Add(admin);
        await context.SaveChangesAsync(ct);

        return admin.Id;
    }

    public async Task<ErrorOr<Success>> ToggleCompanyStatusAsync(ToggleCompanyStatusRequest request, CancellationToken ct)
    {
        if (!userContext.IsSystemAdmin)
        {
            return Error.Forbidden("Auth.Forbidden", "Only system administrators can update company status.");
        }

        var company = await context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, ct);

        if (company is null)
        {
            return Error.NotFound("Company.NotFound", "Company was not found.");
        }

        company.UpdateStatus(request.IsActive);

        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

    public async Task<ErrorOr<bool>> ExistsAsync(string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Error.Validation("Admin.Email.Empty", "Email is required.");
        }

        var normalizedEmail = email.Trim();
        var exists = await context.SystemAdmins.AnyAsync(a => a.Email == normalizedEmail, ct);
        return exists;
    }

    public async Task<ErrorOr<List<SystemAdminResponse>>> GetAllAsync(CancellationToken ct)
    {
        if (!userContext.IsSystemAdmin)
        {
            return Error.Forbidden("Auth.Forbidden", "Access denied.");
        }

        var admins = await context.SystemAdmins
            .AsNoTracking()
            .Select(a => new SystemAdminResponse
            {
                Id = a.Id,
                Login = a.Login,
                Email = a.Email,
                IsRoot = a.IsRoot,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);

        return admins;
    }
}
