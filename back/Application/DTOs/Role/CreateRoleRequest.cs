using Domain.Constants;

namespace Application.DTOs.Role;

public sealed record class CreateRoleRequest
{
    public required string Name { get; init; }
    public required int AccessLevel { get; init; }
    public required List<Permission> Permissions { get; init; }
}