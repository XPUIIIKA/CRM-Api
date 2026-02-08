using Domain.Abstractions;
using Domain.BaseEntities;

namespace Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; }
    public Guid CreatedBy { get; protected set; }

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