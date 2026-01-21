using Application.DTOs.Company;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ICompanyService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateCompanyRequest request, CancellationToken ct);
    Task<ErrorOr<bool>> ExistsAsync(Guid companyId, CancellationToken ct);
}