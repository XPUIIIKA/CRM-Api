namespace Application.DTOs.Order;

public sealed record class UpdateOrderStatusRequest
{
    public required Guid StatusId { get; init; }
}