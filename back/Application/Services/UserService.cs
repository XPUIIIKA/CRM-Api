using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.User;
using Application.Services.Helpers;
using Domain.Constants;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class UserService(
    ICrmDbContext context,
    IHasher passwordHasher,
    ICurrentUserContext userContext) : IUserService
{
    public async Task<ErrorOr<Guid>> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (userId, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Error.Validation("User.Email.Empty", "Email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Error.Validation("User.Password.Empty", "Password cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return Error.Validation("User.FullName.Empty", "Full name cannot be empty.");
        }

        if (!InputValidation.IsValidPhone(request.PhoneNumber))
        {
            return Error.Validation("User.Phone.Invalid", "Invalid phone format.");
        }

        var normalizedEmail = request.Email.Trim();
        if (!InputValidation.IsValidEmail(normalizedEmail))
        {
            return Error.Validation("User.Email.Invalid", "Invalid email format.");
        }

        var normalizedPhone = request.PhoneNumber.Trim();

        var userEmailExists = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail.ToLower(), ct);

        var adminEmailExists = await context.SystemAdmins
            .AnyAsync(a => a.Email.ToLower() == normalizedEmail.ToLower(), ct);

        if (userEmailExists || adminEmailExists)
        {
            return Error.Conflict("User.DuplicateEmail", "Email is already used.");
        }

        var role = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId &&
                                      (r.CompanyId == Guid.Empty || r.CompanyId == companyId), ct);

        if (role is null)
        {
            return Error.NotFound("Role.Invalid", "Role was not found.");
        }

        if (role.AccessLevel >= userContext.AccessLevel)
        {
            return Error.Validation("Auth.HierarchyError", "Cannot create a user with equal or higher access level.");
        }

        var user = new User(
            roleId: request.RoleId,
            email: normalizedEmail,
            passwordHash: passwordHasher.Hash(request.Password),
            fullName: request.FullName.Trim(),
            phoneNumber: normalizedPhone,
            createdBy: userId,
            companyId: companyId);

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        return user.Id;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (currentUserId, companyId) = guardResult.Value;

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId, ct);

        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User was not found.");
        }

        var isSelfUpdate = currentUserId == userId;
        if (!isSelfUpdate && !userContext.Permissions.Contains(Permission.ManageUsers))
        {
            return Error.Forbidden("Auth.Forbidden", "Access denied.");
        }

        var hasAnyFieldToUpdate =
            !string.IsNullOrWhiteSpace(request.FullName) ||
            !string.IsNullOrWhiteSpace(request.Email) ||
            !string.IsNullOrWhiteSpace(request.PhoneNumber) ||
            !string.IsNullOrWhiteSpace(request.Password);

        if (!hasAnyFieldToUpdate)
        {
            return Error.Validation("User.Update.Empty", "No fields provided for update.");
        }

        var fullName = string.IsNullOrWhiteSpace(request.FullName)
            ? user.FullName
            : request.FullName.Trim();
        var email = string.IsNullOrWhiteSpace(request.Email)
            ? user.Email
            : request.Email.Trim();
        var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? user.PhoneNumber
            : request.PhoneNumber.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Error.Validation("User.FullName.Empty", "Full name cannot be empty.");
        }

        if (!InputValidation.IsValidEmail(email))
        {
            return Error.Validation("User.Email.Invalid", "Invalid email format.");
        }

        if (!InputValidation.IsValidPhone(phoneNumber))
        {
            return Error.Validation("User.Phone.Invalid", "Invalid phone format.");
        }

        if (!email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var userEmailExists = await context.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Id != userId && u.Email.ToLower() == email.ToLower(), ct);

            var adminEmailExists = await context.SystemAdmins
                .AnyAsync(a => a.Email.ToLower() == email.ToLower(), ct);

            if (userEmailExists || adminEmailExists)
            {
                return Error.Conflict("User.DuplicateEmail", "Email is already used.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Trim().Length < 8)
            {
                return Error.Validation("User.Password.Weak", "Password must contain at least 8 characters.");
            }

            user.ChangePassword(passwordHasher.Hash(request.Password.Trim()));
        }

        user.UpdateProfile(fullName, email, phoneNumber);
        await context.SaveChangesAsync(ct);

        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> ChangeRoleAsync(Guid userId, Guid newRoleId, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null || user.CompanyId != companyId)
        {
            return Error.NotFound("User.NotFound", "User was not found.");
        }

        var newRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == newRoleId &&
                                      (r.CompanyId == Guid.Empty || r.CompanyId == companyId), ct);

        if (newRole is null)
        {
            return Error.NotFound("Role.NotFound", "Role was not found.");
        }

        if (newRole.AccessLevel >= userContext.AccessLevel)
        {
            return Error.Validation("Role.TooHigh", "Cannot assign a role with equal or higher access level.");
        }

        user.ChangeRole(newRoleId);
        await context.SaveChangesAsync(ct);

        return Result.Updated;
    }

    public async Task<ErrorOr<Success>> DeactivateAsync(Guid userId, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null || user.CompanyId != companyId)
        {
            return Error.NotFound("User.NotFound", "User was not found.");
        }

        if (user.Id == userContext.UserId)
        {
            return Error.Validation("User.SelfDeactivation", "You cannot deactivate yourself.");
        }

        var targetUserRole = await context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == user.RoleId &&
                                      (r.CompanyId == Guid.Empty || r.CompanyId == companyId), ct);

        if (targetUserRole is null)
        {
            return Error.NotFound("Role.NotFound", "Role was not found.");
        }

        if (targetUserRole.AccessLevel >= userContext.AccessLevel)
        {
            return Error.Validation("Auth.HierarchyError", "Insufficient permissions to deactivate this user.");
        }

        user.Deactivate();
        await context.SaveChangesAsync(ct);

        return Result.Success;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return user?.Role?.HasPermission(permission) ?? false;
    }

    public async Task<ErrorOr<List<UserResponse>>> GetCompanyUsersAsync(CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var users = await context.Users
            .Include(u => u.Role)
            .Where(u => u.CompanyId == companyId && u.IsActive)
            .AsNoTracking()
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                AccessLevel = u.Role.AccessLevel,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return users;
    }
}
