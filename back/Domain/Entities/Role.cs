using Domain.Abstractions;
using Domain.BaseEntities;
using Domain.Constants;

namespace Domain.Entities;

public class Role : BaseEntity, IHaveCompany
{
    public string Name { get; private set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Guid CreatedBy { get; private set; }
    public int AccessLevel { get; private set; }
    public List<Permission> Permissions { get; private set; } = new();

    protected Role() { }

    public Role(
        string name,
        Guid createdBy,
        int accessLevel,
        List<Permission> permissions,
        Guid companyId = default)
    {
        Id = Guid.NewGuid();
        Name = name;
        CompanyId = companyId;
        CreatedBy = createdBy;
        AccessLevel = accessLevel;
        Permissions = permissions;
        
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Update(string name, int accessLevel, List<Permission> permissions)
    {
        Name = name;
        AccessLevel = accessLevel;
        Permissions = permissions;
        UpdatedAt = DateTime.UtcNow;
    }
    public bool HasPermission(Permission permission) => Permissions.Contains(permission);
}
