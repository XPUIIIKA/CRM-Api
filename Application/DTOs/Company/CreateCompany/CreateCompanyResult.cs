namespace Application.DTOs.Company.CreateCompany;

public sealed record CreateCompanyResult(
    Guid CompanyId,
    Guid OwnerUserId,
    string GeneratedPassword
);