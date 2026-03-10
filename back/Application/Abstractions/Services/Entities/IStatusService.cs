using Application.DTOs.Status;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IStatusService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateStatusRequest request, CancellationToken ct);
    Task<ErrorOr<List<StatusResponse>>> GetAllForCompanyAsync(CancellationToken ct);
    Task<ErrorOr<Updated>> UpdateAsync(Guid id, UpdateStatusRequest request, CancellationToken ct);
    Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct);
}
