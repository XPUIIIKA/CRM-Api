namespace Application.DTOs.Category;

public sealed record class UpdateCategoryRequest
{
    public required string Name { get; init; }
}
