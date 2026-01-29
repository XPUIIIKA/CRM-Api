using Api.Extensions;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IIdentityService identityService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await identityService.LoginAsync(request, ct);
    
        return result.ToActionResult();
    }
}