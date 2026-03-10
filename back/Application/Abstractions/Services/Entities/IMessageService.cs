using Application.DTOs.Message;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IMessageService
{
    Task<ErrorOr<List<DialogSummaryResponse>>> GetDialogsAsync(CancellationToken ct);
    Task<ErrorOr<List<MessageResponse>>> GetDialogMessagesAsync(Guid dialogId, CancellationToken ct);
    Task<ErrorOr<Guid>> SendAsync(SendMessageRequest request, CancellationToken ct);
}
