using Domain.Constants;

namespace Application.DTOs.Role;

public sealed record class UpdateRolePermissionsRequest
{
    public required Guid RoleId { get; init; }
    public required List<Permission> Permissions { get; init; }
}