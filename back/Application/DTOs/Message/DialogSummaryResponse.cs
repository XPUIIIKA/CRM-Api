namespace Application.DTOs.Message;

public sealed record class DialogSummaryResponse
{
    public required Guid DialogId { get; init; }
    public required Guid OtherUserId { get; init; }
    public required string OtherUserName { get; init; }
    public required string LastMessage { get; init; }
    public required DateTime LastMessageAt { get; init; }
    public required bool HasUnread { get; init; }
    public required int UnreadCount { get; init; }
}
