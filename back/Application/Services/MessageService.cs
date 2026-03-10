using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Message;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class MessageService(
    ICrmDbContext context,
    ICurrentUserContext userContext) : IMessageService
{
    public async Task<ErrorOr<List<DialogSummaryResponse>>> GetDialogsAsync(CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (currentUserId, companyId) = guardResult.Value;

        var messages = await context.Messages
            .AsNoTracking()
            .Where(m => m.CompanyId == companyId && (m.SenderUserId == currentUserId || m.ReceiverUserId == currentUserId))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        if (messages.Count == 0)
        {
            return new List<DialogSummaryResponse>();
        }

        var dialogsData = messages
            .GroupBy(m => m.DialogId)
            .Select(g =>
            {
                var lastMessage = g.OrderByDescending(x => x.CreatedAt).First();
                var otherUserId = lastMessage.SenderUserId == currentUserId
                    ? lastMessage.ReceiverUserId
                    : lastMessage.SenderUserId;

                var unreadCount = g.Count(x => x.ReceiverUserId == currentUserId && !x.IsRead);

                return new
                {
                    DialogId = g.Key,
                    OtherUserId = otherUserId,
                    LastMessage = lastMessage.Text,
                    LastMessageAt = lastMessage.CreatedAt,
                    UnreadCount = unreadCount
                };
            })
            .OrderByDescending(x => x.LastMessageAt)
            .ToList();

        var otherUserIds = dialogsData
            .Select(x => x.OtherUserId)
            .Distinct()
            .ToList();

        var userNames = await context.Users
            .AsNoTracking()
            .Where(u => otherUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var response = dialogsData
            .Select(x => new DialogSummaryResponse
            {
                DialogId = x.DialogId,
                OtherUserId = x.OtherUserId,
                OtherUserName = userNames.GetValueOrDefault(x.OtherUserId, "Unknown user"),
                LastMessage = x.LastMessage,
                LastMessageAt = x.LastMessageAt,
                HasUnread = x.UnreadCount > 0,
                UnreadCount = x.UnreadCount
            })
            .ToList();

        return response;
    }

    public async Task<ErrorOr<List<MessageResponse>>> GetDialogMessagesAsync(Guid dialogId, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (currentUserId, companyId) = guardResult.Value;

        var dialog = await context.Dialogs
            .FirstOrDefaultAsync(d => d.Id == dialogId && d.CompanyId == companyId, ct);

        if (dialog is null)
        {
            return Error.NotFound("Dialog.NotFound", "Dialog was not found.");
        }

        var hasAccess = await context.Messages.AnyAsync(
            m => m.DialogId == dialogId && m.CompanyId == companyId &&
                 (m.SenderUserId == currentUserId || m.ReceiverUserId == currentUserId),
            ct);

        if (!hasAccess && dialog.CreatedBy != currentUserId)
        {
            return Error.Forbidden("Dialog.Forbidden", "Access to this dialog is forbidden.");
        }

        var messages = await context.Messages
            .Where(m => m.DialogId == dialogId && m.CompanyId == companyId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        var senderIds = messages
            .Select(m => m.SenderUserId)
            .Distinct()
            .ToList();

        var senderNames = await context.Users
            .AsNoTracking()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var unreadMessages = messages
            .Where(m => m.ReceiverUserId == currentUserId && !m.IsRead)
            .ToList();

        foreach (var unreadMessage in unreadMessages)
        {
            unreadMessage.MarkAsRead();
        }

        if (unreadMessages.Count > 0)
        {
            await context.SaveChangesAsync(ct);
        }

        var response = messages
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                DialogId = m.DialogId,
                SenderUserId = m.SenderUserId,
                ReceiverUserId = m.ReceiverUserId,
                SenderName = senderNames.GetValueOrDefault(m.SenderUserId, "Unknown user"),
                Text = m.Text,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return response;
    }

    public async Task<ErrorOr<Guid>> SendAsync(SendMessageRequest request, CancellationToken ct)
    {
        var guardResult = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guardResult.IsError)
        {
            return guardResult.Errors;
        }

        var (currentUserId, companyId) = guardResult.Value;

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return Error.Validation("Message.Text.Empty", "Message text cannot be empty.");
        }

        var messageText = request.Text.Trim();
        if (messageText.Length > 2000)
        {
            return Error.Validation("Message.Text.TooLong", "Message is too long.");
        }

        if (request.ReceiverUserId == currentUserId)
        {
            return Error.Validation("Message.Receiver.Self", "You cannot send a message to yourself.");
        }

        var receiverExists = await context.Users
            .AnyAsync(u => u.Id == request.ReceiverUserId && u.CompanyId == companyId && u.IsActive, ct);

        if (!receiverExists)
        {
            return Error.NotFound("User.NotFound", "Receiver was not found.");
        }

        Dialog dialog;
        if (request.DialogId.HasValue)
        {
            var existingDialog = await context.Dialogs
                .FirstOrDefaultAsync(d => d.Id == request.DialogId.Value && d.CompanyId == companyId, ct);

            if (existingDialog is null)
            {
                return Error.NotFound("Dialog.NotFound", "Dialog was not found.");
            }

            dialog = existingDialog;

            var hasDialogAccess = await context.Messages.AnyAsync(
                m => m.DialogId == dialog.Id &&
                     m.CompanyId == companyId &&
                     (m.SenderUserId == currentUserId || m.ReceiverUserId == currentUserId),
                ct);

            if (!hasDialogAccess && dialog.CreatedBy != currentUserId)
            {
                return Error.Forbidden("Dialog.Forbidden", "Access to this dialog is forbidden.");
            }
        }
        else
        {
            var dialogId = await context.Messages
                .AsNoTracking()
                .Where(m => m.CompanyId == companyId &&
                            ((m.SenderUserId == currentUserId && m.ReceiverUserId == request.ReceiverUserId) ||
                             (m.SenderUserId == request.ReceiverUserId && m.ReceiverUserId == currentUserId)))
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => (Guid?)m.DialogId)
                .FirstOrDefaultAsync(ct);

            if (dialogId.HasValue)
            {
                dialog = await context.Dialogs
                    .FirstAsync(d => d.Id == dialogId.Value && d.CompanyId == companyId, ct);
            }
            else
            {
                dialog = new Dialog(companyId, currentUserId);
                context.Dialogs.Add(dialog);
            }
        }

        var message = new Message(
            companyId: companyId,
            dialogId: dialog.Id,
            senderUserId: currentUserId,
            receiverUserId: request.ReceiverUserId,
            text: messageText);

        dialog.Touch();
        context.Messages.Add(message);
        await context.SaveChangesAsync(ct);

        return message.Id;
    }
}
