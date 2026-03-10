using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController(IStatisticsService statisticsService) : ControllerBase
{
    [HttpGet("sales")]
    [HasPermission(Permission.ViewAnalytics)]
    public async Task<IActionResult> GetSales(CancellationToken ct) =>
        (await statisticsService.GetSalesAsync(ct)).ToActionResult();

    [HttpGet("channels")]
    [HasPermission(Permission.ViewAnalytics)]
    public async Task<IActionResult> GetChannels(CancellationToken ct) =>
        (await statisticsService.GetChannelsAsync(ct)).ToActionResult();

    [HttpGet("categories")]
    [HasPermission(Permission.ViewAnalytics)]
    public async Task<IActionResult> GetCategories(CancellationToken ct) =>
        (await statisticsService.GetCategoriesAsync(ct)).ToActionResult();
}
