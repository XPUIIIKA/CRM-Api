namespace Application.DTOs.Order;

public sealed record class CreateOrderRequest
{
    public Guid? ClientId { get; init; }
    public List<OrderItemRequest>? Items { get; init; }
}
public sealed record class OrderItemRequest
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal PriceAtOrder { get; init; }
}