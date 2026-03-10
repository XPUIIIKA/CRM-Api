namespace Application.DTOs.Statistics;

public sealed record class SalesByMonthResponse
{
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required int OrdersCount { get; init; }
    public required decimal TotalSales { get; init; }
}
