using Application.DTOs.Category;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ICategoryService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateCategoryRequest request , CancellationToken ct);
}