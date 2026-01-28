namespace Application.DTOs.Order;

public record OrderItemDto
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal Price { get; init; }
}