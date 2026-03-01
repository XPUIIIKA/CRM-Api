using System.Security.Claims;
using Application.Abstractions.Services.Utils;
using Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Auth;

public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => 
        Guid.TryParse(User?.FindFirst("sub")?.Value, out var id) ? id : null;

    public Guid? CompanyId => 
        Guid.TryParse(User?.FindFirst("company_id")?.Value, out var id) ? id : null;
    public int AccessLevel => 
        int.TryParse(User?.FindFirst("access_level")?.Value, out var level) ? level : (IsSystemAdmin ? 1000 : 0);
    public IReadOnlyList<Permission> Permissions =>
        User?.FindAll("permissions")
            .Select(c => Enum.TryParse<Permission>(c.Value, out var p) ? p : (Permission?)null)
            .Where(p => p.HasValue)
            .Select(p => p!.Value)
            .ToList() ?? new List<Permission>();
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsSystemAdmin => User?.IsInRole("SystemAdmin") ?? false;
}