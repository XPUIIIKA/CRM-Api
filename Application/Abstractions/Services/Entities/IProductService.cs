using Application.DTOs.Product;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IProductService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateProductRequest request ,CancellationToken ct);
}