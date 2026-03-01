namespace Application.DTOs.Product;

public sealed record class ProductResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; } // Новое поле
    public required Guid CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
}