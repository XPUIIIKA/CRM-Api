using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Role;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class RoleService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IRoleService
{
    public async Task<ErrorOr<Guid>> CreateCustomRoleAsync(CreateRoleRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (userId, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Error.Validation("Role.Name.Empty", "Role name cannot be empty.");
        }

        var normalizedRoleName = request.Name.Trim();

        if (request.AccessLevel >= userContext.AccessLevel)
        {
            return Error.Validation("Role.AccessLevelTooHigh",
                $"Cannot create role with access level {request.AccessLevel}. Your level: {userContext.AccessLevel}");
        }

        var roleNameExists = await context.Roles
            .IgnoreQueryFilters()
            .AnyAsync(r => r.CompanyId == companyId && r.Name.ToLower() == normalizedRoleName.ToLower(), ct);

        if (roleNameExists)
        {
            return Error.Conflict("Role.DuplicateName", "Role with this name already exists in the company.");
        }

        var role = new Role(
            name: normalizedRoleName,
            createdBy: userId,
            accessLevel: request.AccessLevel,
            permissions: request.Permissions.Distinct().ToList(),
            companyId: companyId);

        context.Roles.Add(role);
        await context.SaveChangesAsync(ct);

        return role.Id;
    }

    public async Task<ErrorOr<Updated>> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var role = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId &&
                                      (r.CompanyId == Guid.Empty || r.CompanyId == companyId), ct);

        if (role is null)
        {
            return Error.NotFound("Role.NotFound", "Role was not found.");
        }

        if (role.CompanyId == Guid.Empty)
        {
            return Error.Validation("Role.SystemRoleImmutable", "System roles cannot be modified.");
        }

        if (role.AccessLevel >= userContext.AccessLevel)
        {
            return Error.Validation("Role.InsufficientRights", "Insufficient permissions to modify this role.");
        }

        role.Update(role.Name, role.AccessLevel, request.Permissions.Distinct().ToList());
        await context.SaveChangesAsync(ct);

        return Result.Updated;
    }

    public async Task<ErrorOr<List<RoleResponse>>> GetAvailableRolesAsync(CancellationToken ct)
    {
        IQueryable<Role> rolesQuery;

        if (userContext.IsSystemAdmin)
        {
            rolesQuery = context.Roles
                .IgnoreQueryFilters()
                .Where(r => r.CompanyId == Guid.Empty);
        }
        else
        {
            var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
            if (guardResult.IsError)
            {
                return guardResult.Errors;
            }

            var (_, companyId) = guardResult.Value;

            rolesQuery = context.Roles
                .IgnoreQueryFilters()
                .Where(r => r.CompanyId == Guid.Empty || r.CompanyId == companyId);
        }

        var response = await rolesQuery
            .AsNoTracking()
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                AccessLevel = r.AccessLevel,
                Permissions = r.Permissions,
                IsCustom = r.CompanyId != Guid.Empty
            })
            .ToListAsync(ct);

        return response;
    }
}
