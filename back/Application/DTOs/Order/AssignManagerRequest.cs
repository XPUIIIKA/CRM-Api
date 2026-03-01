namespace Application.DTOs.Order;

public sealed record class AssignManagerRequest
{
    public required Guid ManagerId { get; init; }
}