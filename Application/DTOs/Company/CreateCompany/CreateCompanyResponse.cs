namespace Application.DTOs.Company.CreateCompany;

public sealed class CreateCompanyResponse
{
    public required Guid CompanyId { get; init; }
    public required Guid OwnerId { get; init; }
    public required string GeneratedPassword { get; init; }
}