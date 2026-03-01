namespace Application.DTOs.Authorization;


public sealed record class LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}