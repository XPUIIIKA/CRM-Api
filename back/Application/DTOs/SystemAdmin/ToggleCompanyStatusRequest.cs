namespace Application.DTOs.SystemAdmin;

public sealed record class ToggleCompanyStatusRequest
{
    public required Guid CompanyId { get; init; }
    public required bool IsActive { get; init; }
}