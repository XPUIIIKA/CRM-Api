using Domain.Constants;

namespace Application.DTOs.Role;

public sealed record class RoleResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int AccessLevel { get; init; }
    public required List<Permission> Permissions { get; init; }
    public required bool IsCustom { get; init; }
}