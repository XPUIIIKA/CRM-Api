using Application.Abstractions.Services.Utils;
using ErrorOr;

namespace Application.Services.Helpers;

public static class CurrentUserGuard
{
    public static ErrorOr<(Guid UserId, Guid CompanyId)> EnsureUserAndCompany(
        ICurrentUserContext context)
    {
        if (context.UserId is null)
            return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated");

        if (context.CompanyId is null)
            return Error.Forbidden("Company.Forbidden", "User is not associated with a company");

        return (context.UserId.Value, context.CompanyId.Value);
    }
}