namespace Application.DTOs.User;

public class CreateUserRequest
{
    public required string Email { get; init; } = null!;
    public required string Password { get; init; } = null!;
    public required string FullName { get; init; } = null!;
}