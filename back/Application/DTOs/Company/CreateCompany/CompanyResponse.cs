namespace Application.DTOs.Company.CreateCompany;

public sealed record class CompanyResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateTime CreatedAt { get; init; }
    public Guid? OwnerId { get; init; }
}