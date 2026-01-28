namespace Application.DTOs.Company.CreateCompany;

public sealed class CreateCompanyResult
{
    public required Guid CompanyId { get; init; }
    public required Guid OwnerUserId { get; init; }
    public required string GeneratedPassword { get; init; }
}