using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IStatusService
{
    Task<ErrorOr<Guid>> CreateAsync(string name, CancellationToken ct);
}