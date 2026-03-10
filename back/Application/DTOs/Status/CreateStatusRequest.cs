namespace Application.DTOs.Status;

public sealed record class CreateStatusRequest
{
    public required string Name { get; init; }
}
