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
    IJwtTokenGenerator jwtGenerator) : IIdentityService
{
    public async Task<ErrorOr<AuthenticationResult>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var systemAdmin = await context.SystemAdmins
            .FirstOrDefaultAsync(x => x.Email == request.Email, ct);

        if (systemAdmin is not null)
        {
            if (!passwordHasher.Verify(request.Password, systemAdmin.PasswordHash))
                return Error.Validation("Auth.InvalidCredentials", "Неверный email или пароль");

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
            .FirstOrDefaultAsync(x => x.Email == request.Email, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Error.Validation("Auth.InvalidCredentials", "Неверный email или пароль");

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
}