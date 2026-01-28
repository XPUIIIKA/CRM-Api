using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class StatusService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IStatusService
{
    public async Task<ErrorOr<Guid>> CreateAsync(string name, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError) return guard.Errors;

        var (userId, companyId) = guard.Value;

        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Status.Name.Empty", "Status name cannot be empty.");

        var statusExists = await context.Statuses
            .AnyAsync(s => s.Name.ToLower() == name.ToLower() && 
                           s.CompanyId == companyId, ct);

        if (statusExists)
            return Error.Conflict("Status.Name.Exists", $"Status '{name}' already exists in your company.");

        var status = new Status(
            companyId: companyId,
            name: name,
            createdBy: userId
        );

        context.Statuses.Add(status);
        await context.SaveChangesAsync(ct);

        return status.Id;
    }
}