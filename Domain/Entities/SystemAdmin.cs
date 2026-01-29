using Domain.BaseEntities;

namespace Domain.Entities;

public class SystemAdmin : BaseEntity
{
    public string Email { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsRoot { get; set; }
    public SystemAdmin() { }

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