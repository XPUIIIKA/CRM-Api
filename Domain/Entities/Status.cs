using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Status : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; }
    public Guid CreatedBy { get; set; }

    public Status() { }

    public Status(Guid companyId, string name, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        Name = name;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}