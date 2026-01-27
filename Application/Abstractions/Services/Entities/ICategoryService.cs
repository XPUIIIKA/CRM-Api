using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ICategoryService
{
    Task<ErrorOr<Guid>> CreateAsync(string name , CancellationToken ct);
}