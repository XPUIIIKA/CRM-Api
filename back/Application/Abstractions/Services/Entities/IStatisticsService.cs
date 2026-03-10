using Application.DTOs.Statistics;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IStatisticsService
{
    Task<ErrorOr<List<SalesByMonthResponse>>> GetSalesAsync(CancellationToken ct);
    Task<ErrorOr<List<ChannelSalesResponse>>> GetChannelsAsync(CancellationToken ct);
    Task<ErrorOr<List<CategorySalesResponse>>> GetCategoriesAsync(CancellationToken ct);
}
