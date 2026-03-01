using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Role;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/role")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpPost]
    [HasPermission(Permission.ManageRoles)]
    public async Task<IActionResult> Create(CreateRoleRequest request, CancellationToken ct)
    {
        var result = await roleService.CreateCustomRoleAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpPut("permissions")]
    [HasPermission(Permission.ManageRoles)]
    public async Task<IActionResult> UpdatePermissions(UpdateRolePermissionsRequest request, CancellationToken ct)
    {
        var result = await roleService.UpdatePermissionsAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableRoles(CancellationToken ct)
    {
        var result = await roleService.GetAvailableRolesAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("all-permissions")]
    [HasPermission(Permission.ManageRoles)]
    public IActionResult GetAllPermissions()
    {
        var permissions = Enum.GetValues<Permission>()
            .Select(p => new 
            { 
                Id = (int)p, 
                Name = p.ToString(),
                Description = GetPermissionDescription(p)
            })
            .ToList();

        return Ok(permissions);
    }

    private static string GetPermissionDescription(Permission p) => p switch
    {
        Permission.ViewClients => "Просмотр списка клиентов",
        Permission.ManageUsers => "Управление сотрудниками компании",
        _ => p.ToString()
    };
}