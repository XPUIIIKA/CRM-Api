namespace Domain.Entities;

public class Company : BaseEntities.BaseEntity
{
    public string Name { get; private set; }

    protected Company() { }

    public Company(string name, Guid createdBy)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}