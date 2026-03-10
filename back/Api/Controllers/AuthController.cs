using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Authorization;
using Application.DTOs.User;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IIdentityService identityService,
    IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await identityService.LoginAsync(request, ct);
    
        return result.ToActionResult();
    }
    [HttpGet("me")]
    public IActionResult GetMe([FromServices] ICurrentUserContext userContext)
    {
        if (!userContext.IsAuthenticated)
            return Unauthorized();

        return Ok(new
        {
            userContext.UserId,
            userContext.CompanyId,
            userContext.IsSystemAdmin,
            userContext.AccessLevel,
            userContext.Permissions
        });
    }

    [HttpGet("me/full")]
    public async Task<IActionResult> GetMeFull(CancellationToken ct)
    {
        var result = await identityService.GetCurrentUserProfileAsync(ct);
        return result.ToActionResult();
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateUserRequest request,
        [FromServices] ICurrentUserContext userContext,
        CancellationToken ct)
    {
        if (userContext.UserId is null)
        {
            return Unauthorized();
        }

        var result = await userService.UpdateAsync(userContext.UserId.Value, request, ct);
        return result.ToActionResult();
    }
}
