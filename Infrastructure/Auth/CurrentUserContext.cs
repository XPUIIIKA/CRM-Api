using System.Security.Claims;
using Application.Abstractions.Services.Utils;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Auth;

public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => 
        Guid.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    public Guid? CompanyId => 
        Guid.TryParse(User?.FindFirst("company_id")?.Value, out var id) ? id : null;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsSystemAdmin => User?.IsInRole("SystemAdmin") ?? false;
}