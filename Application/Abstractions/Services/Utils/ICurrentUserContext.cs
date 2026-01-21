namespace Application.Abstractions.Services.Utils;

public interface  ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? CompanyId { get; }

    bool IsAuthenticated { get; }
    bool IsSystemAdmin { get; }
}