using Application.DTOs.Authorization;
using ErrorOr;

namespace Application.Abstractions.Services.Utils;

public interface IIdentityService
{
    Task<ErrorOr<AuthenticationResult>> LoginAsync(LoginRequest request, CancellationToken ct);
}