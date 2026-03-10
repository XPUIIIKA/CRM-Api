using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Authorization;
using Domain.Constants;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Helpers;

public class IdentityService(
    ICrmDbContext context,
    IHasher passwordHasher,
    IJwtTokenGenerator jwtGenerator,
    ICurrentUserContext userContext) : IIdentityService
{
    public async Task<ErrorOr<AuthenticationResult>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Error.Validation("Auth.InvalidCredentials", "Invalid email or password.");
        }

        var normalizedEmail = request.Email.Trim();

        var systemAdmin = await context.SystemAdmins
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, ct);

        if (systemAdmin is not null)
        {
            if (!passwordHasher.Verify(request.Password, systemAdmin.PasswordHash))
            {
                return Error.Validation("Auth.InvalidCredentials", "Invalid email or password.");
            }

            return new AuthenticationResult
            {
                UserId = systemAdmin.Id,
                Email = systemAdmin.Email,
                FullName = "System Administrator",
                Token = jwtGenerator.GenerateToken(systemAdmin),
                RoleId = RoleIds.SystemAdmin,
                CompanyId = null,
                IsRoot = systemAdmin.IsRoot
            };
        }

        var user = await context.Users
            .IgnoreQueryFilters()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Error.Validation("Auth.InvalidCredentials", "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Error.Forbidden("Auth.AccountDisabled", "User account is disabled.");
        }

        var companyIsActive = await context.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == user.CompanyId)
            .Select(c => c.IsActive)
            .FirstOrDefaultAsync(ct);

        if (!companyIsActive)
        {
            return Error.Forbidden("Auth.CompanyDisabled", "Company is disabled.");
        }

        return new AuthenticationResult
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Token = jwtGenerator.GenerateToken(user),
            RoleId = user.RoleId,
            CompanyId = user.CompanyId,
            IsRoot = false
        };
    }

    public async Task<ErrorOr<CurrentUserProfileResponse>> GetCurrentUserProfileAsync(CancellationToken ct)
    {
        if (!userContext.IsAuthenticated || userContext.UserId is null)
        {
            return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");
        }

        var currentUserId = userContext.UserId.Value;

        if (userContext.IsSystemAdmin)
        {
            var admin = await context.SystemAdmins
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId, ct);

            if (admin is null)
            {
                return Error.NotFound("Auth.UserNotFound", "Authenticated user was not found.");
            }

            return new CurrentUserProfileResponse
            {
                Id = admin.Id,
                EntityType = "systemAdmin",
                Email = admin.Email,
                FullName = admin.Login,
                Login = admin.Login,
                PhoneNumber = null,
                CompanyId = null,
                IsSystemAdmin = true,
                IsRoot = admin.IsRoot,
                IsActive = true,
                Role = new CurrentUserRoleResponse
                {
                    Id = RoleIds.SystemAdmin,
                    Name = RoleNames.SystemAdmin,
                    AccessLevel = 1000,
                    Permissions = Enum.GetNames<Permission>()
                }
            };
        }

        var user = await context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == currentUserId, ct);

        if (user is null)
        {
            return Error.NotFound("Auth.UserNotFound", "Authenticated user was not found.");
        }

        return new CurrentUserProfileResponse
        {
            Id = user.Id,
            EntityType = "user",
            Email = user.Email,
            FullName = user.FullName,
            Login = null,
            PhoneNumber = user.PhoneNumber,
            CompanyId = user.CompanyId,
            IsSystemAdmin = false,
            IsRoot = false,
            IsActive = user.IsActive,
            Role = new CurrentUserRoleResponse
            {
                Id = user.RoleId,
                Name = user.Role.Name,
                AccessLevel = user.Role.AccessLevel,
                Permissions = user.Role.Permissions
                    .Distinct()
                    .Select(permission => permission.ToString())
                    .ToList()
            }
        };
    }
}
