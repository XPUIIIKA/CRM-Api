namespace Application.DTOs.Message;

public sealed record class MessageResponse
{
    public required Guid Id { get; init; }
    public required Guid DialogId { get; init; }
    public required Guid SenderUserId { get; init; }
    public required Guid ReceiverUserId { get; init; }
    public required string SenderName { get; init; }
    public required string Text { get; init; }
    public required bool IsRead { get; init; }
    public required DateTime CreatedAt { get; init; }
}
