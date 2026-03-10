namespace Application.DTOs.Client;

public sealed record class ClientResponse
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string Surname { get; init; }
    public required string Patronymic { get; init; }
    public required string Phone { get; init; }
    public required string Email { get; init; }
    public required string Address { get; init; }
    public required DateTime CreatedAt { get; init; }
}
