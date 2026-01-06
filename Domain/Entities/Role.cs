namespace Domain.Entities;

public class Role : BaseEntities.BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; }

    protected Role() { }

    public Role(Guid companyId, string name, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        Name = name;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}