namespace Application.DTOs.User;

public sealed record class UpdateUserRequest
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Password { get; init; }
}
