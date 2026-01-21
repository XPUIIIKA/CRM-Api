namespace Application.DTOs.Status;

public class CreateStatusRequest
{
    public required Guid CompanyId { get; init; }
    public required string Name { get; init; }
}