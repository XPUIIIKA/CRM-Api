namespace Application.DTOs.Order;

public sealed record class OrderResponse
{
    public required Guid Id { get; init; }
    public Guid? ClientId { get; init; }
    public string? ClientName { get; init; }
    public string? DeliveryAddress { get; init; }
    public string? Notes { get; init; }
    public string? SalesChannel { get; init; }
    public required Guid? CurrentStatusId { get; init; }
    public string? CurrentStatusName { get; init; }
    public Guid? AssignedManagerId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public List<OrderItemResponse> Items { get; init; } = new();
}

public sealed record class OrderItemResponse
{
    public required Guid ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal Price { get; init; }
}
