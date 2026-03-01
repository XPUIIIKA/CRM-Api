using Application.DTOs.Order;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IOrderService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateOrderRequest request, CancellationToken ct);
    Task<ErrorOr<List<OrderResponse>>> GetAllAsync(CancellationToken ct);
    Task<ErrorOr<Updated>> ChangeStatusAsync(Guid orderId, Guid statusId, CancellationToken ct);
    Task<ErrorOr<Updated>> AssignManagerAsync(Guid orderId, Guid managerId, CancellationToken ct);
    Task<ErrorOr<Updated>> UpdateItemsAsync(Guid orderId, List<OrderItemRequest> items, CancellationToken ct);
}