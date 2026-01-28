namespace Application.DTOs.Product;

public class CreateProductRequest
{
    public required Guid CategoryId { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}