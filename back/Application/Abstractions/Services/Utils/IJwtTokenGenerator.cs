using Domain.Entities;

namespace Application.Abstractions.Services.Utils;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);

    string GenerateToken(SystemAdmin admin);
}