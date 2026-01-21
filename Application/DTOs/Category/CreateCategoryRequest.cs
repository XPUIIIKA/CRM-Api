namespace Application.DTOs.Category;

public class CreateCategoryRequest
{
    public required Guid CompanyId { get; init; }
    public required string Name { get; init; }
}