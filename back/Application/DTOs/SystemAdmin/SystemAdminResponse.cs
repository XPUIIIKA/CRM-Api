namespace Application.DTOs.SystemAdmin;

public sealed record class SystemAdminResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Login { get; init; }
    public required bool IsRoot { get; init; }
    public required DateTime CreatedAt { get; init; }
}