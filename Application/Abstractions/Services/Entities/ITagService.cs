using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ITagService
{
    Task<ErrorOr<Guid>> CreateAsync(string name, CancellationToken ct);
}