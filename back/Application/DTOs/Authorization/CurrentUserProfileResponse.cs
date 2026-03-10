namespace Application.DTOs.Authorization;

public sealed record class CurrentUserRoleResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int AccessLevel { get; init; }
    public required IReadOnlyList<string> Permissions { get; init; }
}

public sealed record class CurrentUserProfileResponse
{
    public required Guid Id { get; init; }
    public required string EntityType { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public string? Login { get; init; }
    public string? PhoneNumber { get; init; }
    public Guid? CompanyId { get; init; }
    public required bool IsSystemAdmin { get; init; }
    public required bool IsRoot { get; init; }
    public required bool IsActive { get; init; }
    public required CurrentUserRoleResponse Role { get; init; }
}
