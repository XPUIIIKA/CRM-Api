namespace Application.DTOs.Order;

public class CreateOrderRequest
{
    public required Guid CompanyId { get; init; }
    public Guid ClientId { get; init; }
    public required Guid CreatedByUserId { get; init; }
}