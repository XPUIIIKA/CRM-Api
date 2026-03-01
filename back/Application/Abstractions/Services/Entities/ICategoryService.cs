using Application.DTOs.Category;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ICategoryService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateCategoryRequest request, CancellationToken ct);
    Task<ErrorOr<List<CategoryResponse>>> GetAllAsync(CancellationToken ct);
    Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct);
}