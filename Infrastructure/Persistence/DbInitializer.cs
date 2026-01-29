using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class DbInitializer(
    ICrmDbContext context, 
    IHasher hasher) : IDbInitializer
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (context is DbContext efContext)
        {
            await efContext.Database.MigrateAsync(ct);
        }

        if (!await context.SystemAdmins.AnyAsync(ct))
        {
            var root = new SystemAdmin
            {
                Id = Guid.NewGuid(),
                Login = "root_admin",
                Email = "admin@crm.com",
                PasswordHash = hasher.Hash("Admin123!"),
                IsRoot = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.SystemAdmins.AddAsync(root, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}