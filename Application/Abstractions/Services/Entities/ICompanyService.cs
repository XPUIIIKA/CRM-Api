using Application.DTOs.Company.CreateCompany;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ICompanyService
{
    Task<ErrorOr<CreateCompanyResult>> CreateAsync(CreateCompanyRequest request, CancellationToken ct);
    Task<ErrorOr<bool>> ExistsAsync(Guid companyId, CancellationToken ct);
}