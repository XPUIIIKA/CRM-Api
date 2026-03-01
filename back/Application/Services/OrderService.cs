using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Order;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class OrderService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IOrderService
{
    public async Task<ErrorOr<Guid>> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (userId, companyId) = guardResult.Value;

        if (request.ClientId.HasValue)
        {
            var clientExists = await context.Clients
                .AnyAsync(c => c.Id == request.ClientId.Value && c.CompanyId == companyId, ct);

            if (!clientExists)
            {
                return Error.NotFound("Client.NotFound", "Client was not found.");
            }
        }

        var items = request.Items ?? [];
        var invalidItem = items.FirstOrDefault(i => i.Quantity <= 0 || i.PriceAtOrder < 0);
        if (invalidItem is not null)
        {
            return Error.Validation("OrderItem.Invalid", "Order item quantity must be greater than zero and price cannot be negative.");
        }

        var requestedProductIds = items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        if (requestedProductIds.Count > 0)
        {
            var existingProductIds = await context.Products
                .AsNoTracking()
                .Where(p => requestedProductIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(ct);

            if (existingProductIds.Count != requestedProductIds.Count)
            {
                return Error.NotFound("Product.NotFound", "One or more products were not found.");
            }
        }

        var order = new Order(createdBy: userId, companyId: companyId);

        if (request.ClientId.HasValue)
        {
            order.AssignClient(request.ClientId.Value);
        }

        foreach (var item in items)
        {
            if (!order.TryAddItem(item.ProductId, item.Quantity, item.PriceAtOrder))
            {
                return Error.Validation("OrderItem.Invalid", "Order item data is invalid.");
            }
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync(ct);

        return order.Id;
    }

    public async Task<ErrorOr<List<OrderResponse>>> GetAllAsync(CancellationToken ct)
    {
        var orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ToListAsync(ct);

        if (orders.Count == 0)
        {
            return new List<OrderResponse>();
        }

        var productIds = orders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var clientIds = orders
            .Where(o => o.ClientId.HasValue)
            .Select(o => o.ClientId!.Value)
            .Distinct()
            .ToList();

        var productNames = await context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var clientNames = await context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => $"{c.FirstName} {c.Surname}".Trim(), ct);

        var response = orders
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                ClientId = o.ClientId,
                ClientName = o.ClientId.HasValue && clientNames.TryGetValue(o.ClientId.Value, out var clientName)
                    ? clientName
                    : null,
                CurrentStatusId = o.CurrentStatusId,
                CreatedAt = o.CreatedAt,
                TotalAmount = o.Items.Sum(i => i.Price * i.Quantity),
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    ProductId = i.ProductId,
                    ProductName = productNames.GetValueOrDefault(i.ProductId, "Unknown product"),
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            })
            .ToList();

        return response;
    }

    public async Task<ErrorOr<Updated>> ChangeStatusAsync(Guid orderId, Guid statusId, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (userId, companyId) = guardResult.Value;

        var statusExists = await context.Statuses
            .AnyAsync(s => s.Id == statusId && (s.CompanyId == companyId || s.CompanyId == Guid.Empty), ct);

        if (!statusExists)
        {
            return Error.NotFound("Status.NotFound", "Status was not found.");
        }

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null || order.CompanyId != companyId)
        {
            return Error.NotFound("Order.NotFound", "Order was not found.");
        }

        order.ChangeStatus(statusId);

        context.OrderStatusHistories.Add(new OrderStatusHistory(
            orderId: order.Id,
            statusId: statusId,
            createdBy: userId,
            companyId: companyId));

        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> AssignManagerAsync(Guid orderId, Guid managerId, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null || order.CompanyId != companyId)
        {
            return Error.NotFound("Order.NotFound", "Order was not found.");
        }

        var managerExists = await context.Users
            .AnyAsync(u => u.Id == managerId && u.CompanyId == companyId && u.IsActive, ct);

        if (!managerExists)
        {
            return Error.NotFound("User.NotFound", "Manager was not found.");
        }

        order.AssignManager(managerId);

        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> UpdateItemsAsync(Guid orderId, List<OrderItemRequest> items, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        items ??= [];

        if (items.Count == 0)
        {
            return Error.Validation("Order.Items.Empty", "Order must include at least one item.");
        }

        var invalidItem = items.FirstOrDefault(i => i.Quantity <= 0 || i.PriceAtOrder < 0);
        if (invalidItem is not null)
        {
            return Error.Validation("OrderItem.Invalid", "Order item quantity must be greater than zero and price cannot be negative.");
        }

        var requestedProductIds = items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var existingProductIds = await context.Products
            .AsNoTracking()
            .Where(p => requestedProductIds.Contains(p.Id) && p.CompanyId == companyId)
            .Select(p => p.Id)
            .ToListAsync(ct);

        if (existingProductIds.Count != requestedProductIds.Count)
        {
            return Error.NotFound("Product.NotFound", "One or more products were not found.");
        }

        var order = await context.Orders
            .AsTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Order was not found.");
        }

        var existingItemsByProductId = order.Items.ToDictionary(i => i.ProductId);
        var dbContext = context as DbContext;

        foreach (var item in items)
        {
            var hasExistingItem = existingItemsByProductId.ContainsKey(item.ProductId);

            if (!order.TryAddItem(item.ProductId, item.Quantity, item.PriceAtOrder, out var addedItem))
            {
                return Error.Validation("OrderItem.Invalid", "Order item data is invalid.");
            }

            if (!hasExistingItem)
            {
                if (addedItem is null)
                {
                    return Error.Unexpected("OrderItem.AddFailed", "Failed to add a new order item.");
                }

                context.OrderItems.Add(addedItem);
                if (dbContext is not null)
                {
                    dbContext.Entry(addedItem).State = EntityState.Added;
                }

                existingItemsByProductId[item.ProductId] = addedItem;
            }
        }

        dbContext?.ChangeTracker.DetectChanges();
        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }
}
