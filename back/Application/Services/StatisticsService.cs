using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Statistics;
using Application.Services.Helpers;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class StatisticsService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IStatisticsService
{
    public async Task<ErrorOr<List<SalesByMonthResponse>>> GetSalesAsync(CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CompanyId == companyId)
            .ToListAsync(ct);

        var result = orders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new SalesByMonthResponse
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                OrdersCount = g.Count(),
                TotalSales = g.Sum(o => o.Items.Sum(i => i.Price * i.Quantity))
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();

        return result;
    }

    public async Task<ErrorOr<List<ChannelSalesResponse>>> GetChannelsAsync(CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CompanyId == companyId)
            .ToListAsync(ct);

        var result = orders
            .GroupBy(o => string.IsNullOrWhiteSpace(o.SalesChannel) ? "Unknown" : o.SalesChannel)
            .Select(g => new ChannelSalesResponse
            {
                Channel = g.Key,
                OrdersCount = g.Count(),
                TotalSales = g.Sum(o => o.Items.Sum(i => i.Price * i.Quantity))
            })
            .OrderByDescending(x => x.TotalSales)
            .ToList();

        return result;
    }

    public async Task<ErrorOr<List<CategorySalesResponse>>> GetCategoriesAsync(CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CompanyId == companyId)
            .ToListAsync(ct);

        var orderItems = orders
            .SelectMany(o => o.Items.Select(i => new { OrderId = o.Id, Item = i }))
            .ToList();

        if (orderItems.Count == 0)
        {
            return new List<CategorySalesResponse>();
        }

        var productIds = orderItems
            .Select(x => x.Item.ProductId)
            .Distinct()
            .ToList();

        var productCategoryMap = await context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.CategoryId, ct);

        var categoryIds = productCategoryMap.Values
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var categoryNames = await context.Categories
            .AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var result = orderItems
            .GroupBy(x =>
            {
                var hasProduct = productCategoryMap.TryGetValue(x.Item.ProductId, out var categoryId);
                return hasProduct ? categoryId : null;
            })
            .Select(g =>
            {
                var categoryId = g.Key;
                var name = categoryId.HasValue && categoryNames.TryGetValue(categoryId.Value, out var n)
                    ? n
                    : "Uncategorized";

                return new CategorySalesResponse
                {
                    CategoryId = categoryId,
                    CategoryName = name,
                    OrdersCount = g.Select(x => x.OrderId).Distinct().Count(),
                    QuantitySold = g.Sum(x => x.Item.Quantity),
                    TotalSales = g.Sum(x => x.Item.Price * x.Item.Quantity)
                };
            })
            .OrderByDescending(x => x.TotalSales)
            .ToList();

        return result;
    }
}
