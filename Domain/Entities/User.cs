using Domain.Abstractions;
using Domain.BaseEntities;
using Domain.Constants;

namespace Domain.Entities;

public class User : BaseEntity, IHaveCompany
{
    public Guid CompanyId { get; set; }
    public Guid RoleId { get; private set; }
    public Guid CreatedBy { get; protected set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }

    protected User() { }

    public User(Guid companyId, Guid roleId, string email, string passwordHash, string fullName, Guid createdBy)
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
}