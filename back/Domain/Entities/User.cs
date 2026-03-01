using Domain.Abstractions;
using Domain.BaseEntities;
using Domain.Constants;

namespace Domain.Entities;

public class User : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid RoleId { get; private set; }
    public Guid CreatedBy { get; protected set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public virtual Role Role { get; private set; } = null!;

    protected User() { }

    public User(
        Guid roleId,
        string email,
        string passwordHash,
        string fullName,
        Guid createdBy,
        Guid companyId = default)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        PasswordHash = passwordHash;
        RoleId = roleId;
        Email = email;
        FullName = fullName;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
    
    public void Deactivate() 
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(Guid newRoleId)
    {
        RoleId = newRoleId;
        UpdatedAt = DateTime.UtcNow;
    }
}
