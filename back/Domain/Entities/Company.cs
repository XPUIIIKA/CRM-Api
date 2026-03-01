using Domain.BaseEntities;

namespace Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = "";
    public Guid CreatedBy { get; protected set; }
    public bool IsActive { get; private set; } = true;

    protected Company() { }

    public Company(string name, Guid createdBy)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        IsActive = true;
    }
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateStatus(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}