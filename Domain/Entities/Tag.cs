using Domain.BaseEntities;

namespace Domain.Entities;

public class Tag : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; }
    public Guid CreatedBy { get; protected set; }
    protected Tag() { }

    public Tag(Guid companyId, string name, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        Name = name;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
