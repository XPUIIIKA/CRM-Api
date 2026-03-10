using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Status;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/statuses")]
public class StatusesController(IStatusService statusService) : ControllerBase
{
    [HttpPost]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> Create([FromBody] CreateStatusRequest request, CancellationToken ct) =>
        (await statusService.CreateAsync(request, ct)).ToActionResult();

    [HttpGet]
    [HasPermission(Permission.ViewAllOrders)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        (await statusService.GetAllForCompanyAsync(ct)).ToActionResult();

    [HttpPut("{id:guid}")]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct) =>
        (await statusService.UpdateAsync(id, request, ct)).ToActionResult();

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await statusService.DeleteAsync(id, ct)).ToActionResult();
}
