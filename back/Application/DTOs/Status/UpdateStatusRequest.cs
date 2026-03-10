namespace Application.DTOs.Status;

public sealed record class UpdateStatusRequest
{
    public required string Name { get; init; }
}
