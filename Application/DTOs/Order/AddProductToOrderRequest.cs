namespace Application.DTOs.Order;

public class AddProductToOrderRequest
{
    public required Guid OrderId { get; init; }
    public required Guid ProductId { get; init; }
    public required Guid Quantity { get; init; }
}