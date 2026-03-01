namespace Application.DTOs.Product;

public sealed record class CreateProductRequest
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public Guid? CategoryId { get; init; }
}