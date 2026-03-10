using Application.DTOs.Client;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IClientService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateClientRequest request , CancellationToken ct);
    Task<ErrorOr<List<ClientResponse>>> GetAllAsync(CancellationToken ct);
    Task<ErrorOr<ClientResponse>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ErrorOr<Updated>> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct);
    Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct);
}
