using Domain.BaseEntities;

namespace Domain.Entities;

public class SystemAdmin : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string Login { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    protected SystemAdmin() { }

    public SystemAdmin(string email, string login, string passwordHash)
    {
        Id = Guid.NewGuid();
        Login = login;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}