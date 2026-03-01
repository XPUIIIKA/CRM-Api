using Application.DTOs.Client;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IClientService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateClientRequest request , CancellationToken ct);
}