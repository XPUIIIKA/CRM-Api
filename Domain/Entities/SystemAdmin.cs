using Domain.BaseEntities;

namespace Domain.Entities;

public class SystemAdmin : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string Login { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsRoot { get; private set; }
    protected SystemAdmin() { }

    public SystemAdmin(string email, string login, string passwordHash, bool isRoot = false)
    {
        Id = Guid.NewGuid();
        Email = email;
        Login = login;
        PasswordHash = passwordHash;
        IsRoot = isRoot;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}