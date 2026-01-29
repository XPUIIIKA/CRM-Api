namespace Application.DTOs.Authorization;

public class AuthenticationResult
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Token { get; init; }
    public required Guid RoleId { get; init; }
    public Guid? CompanyId { get; init; }
    public bool IsRoot { get; init; }
}
