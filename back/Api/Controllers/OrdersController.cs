using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Order;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await orderService.CreateAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    [HasPermission(Permission.ViewAllOrders)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await orderService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
    {
        var result = await orderService.UpdateAsync(id, request, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await orderService.DeleteAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var result = await orderService.ChangeStatusAsync(id, request.StatusId, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/assign")]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignManagerRequest request, CancellationToken ct)
    {
        var result = await orderService.AssignManagerAsync(id, request.ManagerId, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/items")]
    [HasPermission(Permission.ManageOrders)]
    public async Task<IActionResult> UpdateItems(Guid id, [FromBody] List<OrderItemRequest> items, CancellationToken ct)
    {
        var result = await orderService.UpdateItemsAsync(id, items, ct);
        return result.ToActionResult();
    }
}
