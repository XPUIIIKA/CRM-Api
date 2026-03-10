using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Status;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class StatusService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IStatusService
{
    public async Task<ErrorOr<List<StatusResponse>>> GetAllForCompanyAsync(CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var statuses = await context.Statuses
            .AsNoTracking()
            .Where(s => s.CompanyId == Guid.Empty || s.CompanyId == companyId)
            .OrderBy(s => s.CompanyId != Guid.Empty)
            .ThenBy(s => s.Name)
            .Select(s => new StatusResponse
            {
                Id = s.Id,
                Name = s.Name,
                IsSystem = s.CompanyId == Guid.Empty
            })
            .ToListAsync(ct);

        return statuses;
    }

    public async Task<ErrorOr<Guid>> CreateAsync(CreateStatusRequest request, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError) return guard.Errors;

        var (userId, companyId) = guard.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Error.Validation("Status.Name.Empty", "Status name cannot be empty.");

        var normalizedName = request.Name.Trim();

        var statusExists = await context.Statuses
            .AnyAsync(s => s.Name.ToLower() == normalizedName.ToLower() &&
                           s.CompanyId == companyId, ct);

        if (statusExists)
            return Error.Conflict("Status.Name.Exists", $"Status '{normalizedName}' already exists in your company.");

        var status = new Status(
            companyId: companyId,
            name: normalizedName,
            createdBy: userId
        );

        context.Statuses.Add(status);
        await context.SaveChangesAsync(ct);

        return status.Id;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, UpdateStatusRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Error.Validation("Status.Name.Empty", "Status name cannot be empty.");
        }

        var normalizedName = request.Name.Trim();

        var status = await context.Statuses
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (status is null)
        {
            return Error.NotFound("Status.NotFound", "Status was not found.");
        }

        if (status.CompanyId == Guid.Empty)
        {
            return Error.Validation("Status.SystemImmutable", "System statuses cannot be renamed.");
        }

        if (status.CompanyId != companyId)
        {
            return Error.NotFound("Status.NotFound", "Status was not found.");
        }

        var duplicateExists = await context.Statuses.AnyAsync(
            s => s.CompanyId == companyId &&
                 s.Id != id &&
                 s.Name.ToLower() == normalizedName.ToLower(),
            ct);

        if (duplicateExists)
        {
            return Error.Conflict("Status.Name.Exists", $"Status '{normalizedName}' already exists in your company.");
        }

        status.Rename(normalizedName);
        await context.SaveChangesAsync(ct);

        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var status = await context.Statuses
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (status is null)
        {
            return Error.NotFound("Status.NotFound", "Status was not found.");
        }

        if (status.CompanyId == Guid.Empty)
        {
            return Error.Validation("Status.SystemImmutable", "System statuses cannot be deleted.");
        }

        if (status.CompanyId != companyId)
        {
            return Error.NotFound("Status.NotFound", "Status was not found.");
        }

        var usedInOrders = await context.Orders
            .AsNoTracking()
            .AnyAsync(o => o.CompanyId == companyId && o.CurrentStatusId == id, ct);

        if (usedInOrders)
        {
            return Error.Conflict("Status.InUse", "Status is used in orders and cannot be deleted.");
        }

        var usedInStatusHistory = await context.OrderStatusHistories
            .AsNoTracking()
            .AnyAsync(h => h.CompanyId == companyId && h.StatusId == id, ct);

        if (usedInStatusHistory)
        {
            return Error.Conflict("Status.InUse", "Status is used in order history and cannot be deleted.");
        }

        context.Statuses.Remove(status);
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
