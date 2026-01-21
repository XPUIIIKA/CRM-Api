using Application.DTOs.Order;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IOrderService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateOrderRequest request, CancellationToken ct);

    Task<ErrorOr<bool>> AddProductAsync(AddProductToOrderRequest request, CancellationToken ct);
}