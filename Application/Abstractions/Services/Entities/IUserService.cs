using Application.DTOs.User;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IUserService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateUserRequest request, CancellationToken ct);

    Task<ErrorOr<bool>> CanDeleteAsync(Guid userId, CancellationToken ct);
}