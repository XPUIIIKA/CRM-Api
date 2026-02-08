using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Company.CreateCompany;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/companies")]
public class CompaniesController(ICompanyService companyService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        var result = await companyService.CreateAsync(request, ct);
        
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await companyService.GetByIdAsync(id, ct);
        
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await companyService.GetAllAsync(page, pageSize, ct);
        
        return result.ToActionResult();
    }
}