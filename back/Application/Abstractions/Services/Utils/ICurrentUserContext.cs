using Domain.Constants;

namespace Application.Abstractions.Services.Utils;

public interface  ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? CompanyId { get; }
    int AccessLevel { get; }
    
    IReadOnlyList<Permission> Permissions { get; }
    
    bool IsAuthenticated { get; }
    bool IsSystemAdmin { get; }
}