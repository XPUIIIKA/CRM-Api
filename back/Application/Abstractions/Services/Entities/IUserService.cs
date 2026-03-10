using Application.DTOs.User;
using Domain.Constants;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IUserService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task<ErrorOr<Updated>> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct);
    
    Task<ErrorOr<Updated>> ChangeRoleAsync(Guid userId, Guid newRoleId, CancellationToken ct);
    
    Task<ErrorOr<Success>> DeactivateAsync(Guid userId, CancellationToken ct);

    Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken ct);
    Task<ErrorOr<List<UserResponse>>> GetCompanyUsersAsync(CancellationToken ct);
}
