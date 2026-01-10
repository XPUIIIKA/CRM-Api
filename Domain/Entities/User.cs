namespace Domain.Entities;

public class User : BaseEntities.BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid CreatedBy { get; protected set; }
    public string Email { get; private set; }
    public string FullName { get; private set; }

    protected User() { }

    public User(Guid companyId, Guid roleId, string email, string fullName, Guid createdBy)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        RoleId = roleId;
        Email = email;
        FullName = fullName;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}