namespace Application.DTOs.Category;

public sealed record class CreateCategoryRequest
{
    public required string Name { get; init; }
}