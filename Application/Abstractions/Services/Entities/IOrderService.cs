using Application.DTOs.Order;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IOrderService
{
    Task<ErrorOr<Guid>> CreateDraftAsync(CancellationToken ct);
    Task<ErrorOr<bool>> AddItemsAsync(Guid orderId, IEnumerable<OrderItemDto> items, CancellationToken ct);
    Task<ErrorOr<bool>> SetCustomerAsync(Guid orderId, Guid customerId, CancellationToken ct);
    Task<ErrorOr<bool>> ConfirmOrderAsync(Guid orderId, CancellationToken ct);
}