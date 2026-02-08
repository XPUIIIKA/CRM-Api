using Application.DTOs._General;
using Application.DTOs.Company.CreateCompany;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ICompanyService
{
    Task<ErrorOr<CreateCompanyResponse>> CreateAsync(CreateCompanyRequest request, CancellationToken ct);
    
    Task<ErrorOr<CompanyResponse>> GetByIdAsync(Guid id, CancellationToken ct);
    
    Task<ErrorOr<PagedList<CompanyResponse>>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    
    Task<ErrorOr<Deleted>> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct);

    Task<ErrorOr<bool>> ExistsAsync(Guid companyId, CancellationToken ct);
}