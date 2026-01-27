namespace Application.DTOs.Order;

public record OrderItemDto(
    Guid ProductId,
    int Quantity,
    decimal Price
);