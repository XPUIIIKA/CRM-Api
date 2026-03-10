namespace Application.DTOs.Statistics;

public sealed record class ChannelSalesResponse
{
    public required string Channel { get; init; }
    public required int OrdersCount { get; init; }
    public required decimal TotalSales { get; init; }
}
