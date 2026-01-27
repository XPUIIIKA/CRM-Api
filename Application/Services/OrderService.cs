using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Order;
using Application.Services.Helpers;
using Domain.Constants;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class OrderService(
    ICrmDbContext context, 
    ICurrentUserContext userContext) : IOrderService
{
    public async Task<ErrorOr<Guid>> CreateDraftAsync(CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError) return guard.Errors;

        var (userId, companyId) = guard.Value;

        var order = new Order(
            companyId: companyId,
            createdBy: userId
        );
        
        order.ChangeStatus(OrderStatusIds.Draft);
        
        context.Orders.Add(order);
        await context.SaveChangesAsync(ct);

        return order.Id;
    }

    public async Task<ErrorOr<bool>> AddItemsAsync(Guid orderId, IEnumerable<OrderItemDto> items, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError) return guard.Errors;
    
        var order = await context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.CompanyId == guard.Value.CompanyId, ct);

        if (order is null)
            return Error.NotFound("Order.NotFound", "Order not found");

        foreach (var item in items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.Price);
        }

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ErrorOr<bool>> SetCustomerAsync(Guid orderId, Guid customerId, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError) return guard.Errors;

        var order = await context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId && x.CompanyId == guard.Value.CompanyId, ct);

        if (order is null) return Error.NotFound("Order.NotFound");

        var clientExists = await context.Clients
            .AnyAsync(x => x.Id == customerId && x.CompanyId == guard.Value.CompanyId, ct);

        if (!clientExists)
            return Error.NotFound("Client.NotFound", "Client does not exist in your company");

        order.SetCustomer(customerId);
        
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ErrorOr<bool>> ConfirmOrderAsync(Guid orderId, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError) return guard.Errors;

        var order = await context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.CompanyId == guard.Value.CompanyId, ct);

        if (order is null) return Error.NotFound();

        if (!order.CanBeConfirmed)
            return Error.Conflict("Order.NotReady", "Заказ не может быть подтвержден (проверьте товары и клиента)");

        order.Confirm();
        await context.SaveChangesAsync(ct);
    
        return true;
    }
}