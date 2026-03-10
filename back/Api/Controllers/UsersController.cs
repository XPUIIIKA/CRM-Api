using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.User;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/user")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    [HasPermission(Permission.ManageUsers)]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
    {
        var result = await userService.CreateAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanyUsers(CancellationToken ct)
    {
        var result = await userService.GetCompanyUsersAsync(ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var result = await userService.UpdateAsync(id, request, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/role")]
    [HasPermission(Permission.ManageRoles)]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] Guid newRoleId, CancellationToken ct)
    {
        var result = await userService.ChangeRoleAsync(id, newRoleId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.ManageUsers)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await userService.DeactivateAsync(id, ct);
        return result.ToActionResult();
    }
}
