namespace Application.DTOs.Client;

public sealed record class CreateClientRequest
{
    public required string FirstName { get; init; }
    public string Surname { get; init; } = "";
    public string Patronymic { get; init; } = "";
    public string Phone { get; init; } = "";
    public string Email { get; init; } = "";
    public string Address { get; init; } = "";
}
