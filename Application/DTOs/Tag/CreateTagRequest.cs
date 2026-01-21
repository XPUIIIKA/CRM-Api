namespace Application.DTOs.Tag;

public class CreateTagRequest
{
    public required Guid CompanyId { get; init; }
    public required string Name { get; init; }
}