namespace Application.DTOs.User;

public sealed record class CreateUserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FullName { get; init; }
    public string PhoneNumber { get; init; } = "";
    public required Guid RoleId { get; init; }
}
