using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Category;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/category")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpPost]
    [HasPermission(Permission.ManageProducts)]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken ct) =>
        (await categoryService.CreateAsync(request, ct)).ToActionResult();

    [HttpGet]
    [HasPermission(Permission.ViewProducts)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        (await categoryService.GetAllAsync(ct)).ToActionResult();

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.ManageProducts)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await categoryService.DeleteAsync(id, ct)).ToActionResult();
}