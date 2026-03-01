namespace Application.DTOs.SystemAdmin;

public sealed record class CreateSystemAdminRequest
{
    public required string Email { get; init; }
    public required string Login { get; init; }
    public required string Password { get; init; }
    public required bool IsRoot { get; init; }
    
}