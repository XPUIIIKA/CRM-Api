using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Client;
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

        var resolvedClientId = await ResolveClientIdForCreateAsync(
            request.ClientId,
            request.Client,
            userId,
            companyId,
            ct);

        if (resolvedClientId.IsError)
        {
            return resolvedClientId.Errors;
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
                .Where(p => requestedProductIds.Contains(p.Id) && p.CompanyId == companyId)
                .Select(p => p.Id)
                .ToListAsync(ct);

            if (existingProductIds.Count != requestedProductIds.Count)
            {
                return Error.NotFound("Product.NotFound", "One or more products were not found.");
            }
        }

        var order = new Order(createdBy: userId, companyId: companyId);
        order.UpdateMainInfo(request.DeliveryAddress, request.Notes, request.SalesChannel);

        if (resolvedClientId.Value.HasValue)
        {
            order.AssignClient(resolvedClientId.Value.Value);
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
                DeliveryAddress = o.DeliveryAddress,
                Notes = o.Notes,
                SalesChannel = o.SalesChannel,
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

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid orderId, UpdateOrderRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (userId, companyId) = guardResult.Value;

        if (request.ClientId.HasValue && request.Client is not null)
        {
            return Error.Validation("Order.Client.Invalid", "Specify either clientId or client object, not both.");
        }

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Order was not found.");
        }

        if (request.ClientId.HasValue)
        {
            var clientExists = await context.Clients
                .AnyAsync(c => c.Id == request.ClientId.Value && c.CompanyId == companyId, ct);

            if (!clientExists)
            {
                return Error.NotFound("Client.NotFound", "Client was not found.");
            }

            order.AssignClient(request.ClientId.Value);
        }
        else if (request.Client is not null)
        {
            if (order.ClientId.HasValue)
            {
                var existingClient = await context.Clients
                    .FirstOrDefaultAsync(c => c.Id == order.ClientId.Value && c.CompanyId == companyId, ct);

                if (existingClient is null)
                {
                    return Error.NotFound("Client.NotFound", "Client was not found.");
                }

                var normalizedEmail = NormalizeEmail(request.Client.Email);
                if (!string.IsNullOrWhiteSpace(normalizedEmail) && !InputValidation.IsValidEmail(normalizedEmail))
                {
                    return Error.Validation("Client.Email.Invalid", "Invalid email format.");
                }

                if (!InputValidation.IsValidPhone(request.Client.Phone))
                {
                    return Error.Validation("Client.Phone.Invalid", "Invalid phone format.");
                }

                if (!string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    var duplicateEmailExists = await context.Clients.AnyAsync(
                        c => c.CompanyId == companyId &&
                             c.Id != existingClient.Id &&
                             c.Email.ToLower() == normalizedEmail.ToLower(),
                        ct);

                    if (duplicateEmailExists)
                    {
                        return Error.Conflict("Client.EmailExists", "Client with this email already exists.");
                    }
                }

                existingClient.UpdatePersonalData(
                    firstName: request.Client.FirstName.Trim(),
                    surname: request.Client.Surname.Trim(),
                    patronymic: request.Client.Patronymic.Trim(),
                    phone: request.Client.Phone.Trim(),
                    email: normalizedEmail,
                    address: request.Client.Address.Trim());
            }
            else
            {
                var createdClientId = await CreateClientAsync(request.Client, userId, companyId, ct);
                if (createdClientId.IsError)
                {
                    return createdClientId.Errors;
                }

                order.AssignClient(createdClientId.Value);
            }
        }

        order.UpdateMainInfo(
            request.DeliveryAddress ?? order.DeliveryAddress,
            request.Notes ?? order.Notes,
            request.SalesChannel ?? order.SalesChannel);

        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid orderId, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (_, companyId) = guardResult.Value;

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CompanyId == companyId, ct);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Order was not found.");
        }

        var orderHistory = await context.OrderStatusHistories
            .Where(h => h.OrderId == orderId && h.CompanyId == companyId)
            .ToListAsync(ct);

        if (orderHistory.Count > 0)
        {
            context.OrderStatusHistories.RemoveRange(orderHistory);
        }

        context.Orders.Remove(order);
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
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

    private async Task<ErrorOr<Guid?>> ResolveClientIdForCreateAsync(
        Guid? clientId,
        CreateClientRequest? client,
        Guid userId,
        Guid companyId,
        CancellationToken ct)
    {
        if (clientId.HasValue && client is not null)
        {
            return Error.Validation("Order.Client.Invalid", "Specify either clientId or client object, not both.");
        }

        if (clientId.HasValue)
        {
            var clientExists = await context.Clients
                .AnyAsync(c => c.Id == clientId.Value && c.CompanyId == companyId, ct);

            if (!clientExists)
            {
                return Error.NotFound("Client.NotFound", "Client was not found.");
            }

            return clientId;
        }

        if (client is null)
        {
            return (Guid?)null;
        }

        var createdClientId = await CreateClientAsync(client, userId, companyId, ct);
        if (createdClientId.IsError)
        {
            return createdClientId.Errors;
        }

        return createdClientId.Value;
    }

    private async Task<ErrorOr<Guid>> CreateClientAsync(
        CreateClientRequest request,
        Guid userId,
        Guid companyId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return Error.Validation("Client.FirstName.Empty", "Client first name cannot be empty.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        if (!string.IsNullOrWhiteSpace(normalizedEmail) && !InputValidation.IsValidEmail(normalizedEmail))
        {
            return Error.Validation("Client.Email.Invalid", "Invalid email format.");
        }

        if (!InputValidation.IsValidPhone(request.Phone))
        {
            return Error.Validation("Client.Phone.Invalid", "Invalid phone format.");
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var emailExists = await context.Clients.AnyAsync(
                x => x.CompanyId == companyId && x.Email.ToLower() == normalizedEmail.ToLower(),
                ct);

            if (emailExists)
            {
                return Error.Conflict("Client.EmailExists", "Client with this email already exists.");
            }
        }

        var client = new Client(companyId: companyId, createdBy: userId);
        client.UpdatePersonalData(
            firstName: request.FirstName.Trim(),
            surname: request.Surname.Trim(),
            patronymic: request.Patronymic.Trim(),
            phone: request.Phone.Trim(),
            email: normalizedEmail,
            address: request.Address.Trim());

        context.Clients.Add(client);
        return client.Id;
    }

    private static string NormalizeEmail(string email) => email?.Trim() ?? string.Empty;
}
