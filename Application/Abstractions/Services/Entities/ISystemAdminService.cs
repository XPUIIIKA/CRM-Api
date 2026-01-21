using Application.DTOs.SystemAdmin;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ISystemAdminService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateSystemAdminRequest request, CancellationToken ct);

    Task<ErrorOr<bool>> ExistsAsync(string email, CancellationToken ct);
}