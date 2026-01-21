using Application.DTOs.Status;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IStatusService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateStatusRequest request, CancellationToken ct);
}