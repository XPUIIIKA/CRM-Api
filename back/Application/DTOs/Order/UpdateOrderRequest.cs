using Application.DTOs.Client;

namespace Application.DTOs.Order;

public sealed record class UpdateOrderRequest
{
    public Guid? ClientId { get; init; }
    public CreateClientRequest? Client { get; init; }
    public string? DeliveryAddress { get; init; }
    public string? Notes { get; init; }
    public string? SalesChannel { get; init; }
    public Guid StatusId { get; init; }
    public Guid? ManagerId { get; init; }
}
