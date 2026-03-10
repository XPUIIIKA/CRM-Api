namespace Application.DTOs.Statistics;

public sealed record class CategorySalesResponse
{
    public Guid? CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required int OrdersCount { get; init; }
    public required int QuantitySold { get; init; }
    public required decimal TotalSales { get; init; }
}
