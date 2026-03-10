namespace Application.DTOs.Message;

public sealed record class SendMessageRequest
{
    public Guid? DialogId { get; init; }
    public required Guid ReceiverUserId { get; init; }
    public required string Text { get; init; }
}
