namespace Application.DTOs.Company.CreateCompany;

public sealed class CreateCompanyRequest
{
    public required string CompanyName { get; init; }
    public required string OwnerFullName { get; init; }
    public required string OwnerEmail { get; init; }
}
