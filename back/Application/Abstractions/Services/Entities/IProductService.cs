using Application.DTOs.Product;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface IProductService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ErrorOr<List<ProductResponse>>> GetAllAsync(CancellationToken ct);
    Task<ErrorOr<ProductResponse>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ErrorOr<Updated>> UpdateAsync(UpdateProductRequest request, CancellationToken ct);
    Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct);
}