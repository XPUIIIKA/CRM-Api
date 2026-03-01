using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.SystemAdmin;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/system-admin")]
[Authorize(Roles = RoleNames.SystemAdmin)] 
public class SystemAdminController(ISystemAdminService adminService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAdmin(CreateSystemAdminRequest request, CancellationToken ct)
    {
        var result = await adminService.CreateAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpPatch("companies/status")]
    public async Task<IActionResult> ToggleCompanyStatus(ToggleCompanyStatusRequest request, CancellationToken ct)
    {
        var result = await adminService.ToggleCompanyStatusAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpGet("exists")]
    public async Task<IActionResult> CheckExists([FromQuery] string email, CancellationToken ct)
    {
        var result = await adminService.ExistsAsync(email, ct);
        return result.ToActionResult();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await adminService.GetAllAsync(ct);
        return result.ToActionResult();
    }
}