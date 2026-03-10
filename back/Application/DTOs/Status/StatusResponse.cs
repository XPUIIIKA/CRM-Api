namespace Application.DTOs.Status;

public sealed record class StatusResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required bool IsSystem { get; init; }
}
