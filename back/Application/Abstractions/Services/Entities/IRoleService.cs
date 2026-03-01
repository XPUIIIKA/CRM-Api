using Application.DTOs.Role;
using Domain.Constants;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IRoleService
{
    Task<ErrorOr<Guid>> CreateCustomRoleAsync(CreateRoleRequest request, CancellationToken ct);
    Task<ErrorOr<Updated>> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken ct);
    Task<ErrorOr<List<RoleResponse>>> GetAvailableRolesAsync(CancellationToken ct);
}