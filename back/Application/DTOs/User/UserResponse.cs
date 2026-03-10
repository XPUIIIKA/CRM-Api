namespace Application.DTOs.User;

public sealed record class UserResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string PhoneNumber { get; init; }
    public required Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public required int AccessLevel { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
}
