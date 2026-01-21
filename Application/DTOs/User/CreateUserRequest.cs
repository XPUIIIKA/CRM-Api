namespace Application.DTOs.User;

public class CreateUserRequest
{
    public required Guid CompanyId { get; init; }
    public required string Email { get; init; } = null!;
    public required string PasswordHash { get; init; } = null!;
    public required string FullName { get; init; } = null!;
    
}