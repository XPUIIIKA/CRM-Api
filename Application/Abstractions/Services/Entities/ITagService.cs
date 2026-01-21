using Application.DTOs.Tag;
using ErrorOr;

namespace Application.Abstractions.Services.Entities;

public interface ITagService
{
    Task<ErrorOr<Guid>> CreateAsync(CreateTagRequest request, CancellationToken ct);
}