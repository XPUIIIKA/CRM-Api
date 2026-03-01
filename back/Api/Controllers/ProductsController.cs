using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Product;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/product")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpPost]
    [HasPermission(Permission.ManageProducts)]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var result = await productService.CreateAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    [HasPermission(Permission.ViewProducts)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await productService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permission.ViewProducts)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await productService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permission.ManageProducts)]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        if (id != request.Id)
            return BadRequest("ID в URL и в теле запроса не совпадают.");

        var result = await productService.UpdateAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.ManageProducts)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await productService.DeleteAsync(id, ct);
        return result.ToActionResult();
    }
}