using Api.Extensions;
using Application.Abstractions.Services.Entities;
using Application.DTOs.Message;
using Domain.Constants;
using Infrastructure.Auth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    [HttpGet("dialogs")]
    [HasPermission(Permission.ManageTasks)]
    public async Task<IActionResult> GetDialogs(CancellationToken ct) =>
        (await messageService.GetDialogsAsync(ct)).ToActionResult();

    [HttpGet("{dialogId:guid}")]
    [HasPermission(Permission.ManageTasks)]
    public async Task<IActionResult> GetMessages(Guid dialogId, CancellationToken ct) =>
        (await messageService.GetDialogMessagesAsync(dialogId, ct)).ToActionResult();

    [HttpPost]
    [HasPermission(Permission.ManageTasks)]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request, CancellationToken ct) =>
        (await messageService.SendAsync(request, ct)).ToActionResult();
}
