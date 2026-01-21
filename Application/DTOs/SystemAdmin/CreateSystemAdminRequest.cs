namespace Application.DTOs.SystemAdmin;

public sealed class CreateSystemAdminRequest
{
    public required string Email { get; init; } = null!;
    public required string Login { get; init; } = null!;
    public required string Password { get; init; } = null!;
}