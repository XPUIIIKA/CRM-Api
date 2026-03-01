namespace Application.DTOs.Product;

public sealed record class UpdateProductRequest
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public Guid? CategoryId { get; init; }
}