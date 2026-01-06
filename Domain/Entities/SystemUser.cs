using Domain.BaseEntities;

namespace Domain.Entities;

public class SystemUser : BaseEntity
{
    public string Email { get; private set; }
    public bool IsSuperAdmin { get; private set; }

    protected SystemUser() { }

    public SystemUser(string email, bool isSuperAdmin)
    {
        Id = Guid.NewGuid();
        Email = email;
        IsSuperAdmin = isSuperAdmin;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}