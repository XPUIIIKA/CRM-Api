using Domain.BaseEntities;

namespace Domain.Entities;

public class Status : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; }

    protected Status() { }

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