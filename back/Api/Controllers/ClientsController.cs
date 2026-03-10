using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Client;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permission.ViewClients)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        (await clientService.GetAllAsync(ct)).ToActionResult();

    [HttpPost]
    [HasPermission(Permission.CreateClient)]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken ct) =>
        (await clientService.CreateAsync(request, ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    [HasPermission(Permission.ViewClients)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        (await clientService.GetByIdAsync(id, ct)).ToActionResult();

    [HttpPut("{id:guid}")]
    [HasPermission(Permission.EditClient)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request, CancellationToken ct) =>
        (await clientService.UpdateAsync(id, request, ct)).ToActionResult();

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.DeleteClient)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await clientService.DeleteAsync(id, ct)).ToActionResult();
}
