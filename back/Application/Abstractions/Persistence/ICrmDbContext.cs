using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Persistence;

public interface ICrmDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Client> Clients { get; }
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Tag> Tags { get; }
    DbSet<EntityTag> EntityTags { get; }
    DbSet<Status> Statuses { get; }
    DbSet<SystemAdmin> SystemAdmins { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}