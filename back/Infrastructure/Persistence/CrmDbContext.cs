using System.Linq.Expressions;
using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Utils;
using Domain.Abstractions;
using Domain.BaseEntities;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class CrmDbContext(DbContextOptions<CrmDbContext> options, ICurrentUserContext currentUserContext)
    : DbContext(options), ICrmDbContext
{
    public ICurrentUserContext CurrentUser => currentUserContext;

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<EntityTag> EntityTags => Set<EntityTag>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<SystemAdmin> SystemAdmins => Set<SystemAdmin>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHaveCompany).IsAssignableFrom(entityType.ClrType))
            {
                ConfigureGlobalFilters(modelBuilder, entityType.ClrType);
            }
        }
    }

    private void ConfigureGlobalFilters(ModelBuilder modelBuilder, Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(IHaveCompany.CompanyId));

        var contextExpression = Expression.Constant(this);
        var currentUserProp = Expression.Property(contextExpression, nameof(CurrentUser));
        var tenantIdValue = Expression.Property(currentUserProp, nameof(ICurrentUserContext.CompanyId));

        var propertyAsNullable = Expression.Convert(property, typeof(Guid?));
        var compareWithTenant = Expression.Equal(propertyAsNullable, tenantIdValue);
        var isGlobal = Expression.Equal(property, Expression.Constant(Guid.Empty));
        var combinedFilter = Expression.OrElse(compareWithTenant, isGlobal);

        var lambda = Expression.Lambda(combinedFilter, parameter);
        modelBuilder.Entity(entityType).HasQueryFilter(lambda);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        ApplyTenantOwnership();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }

                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }

    private void ApplyTenantOwnership()
    {
        foreach (var entry in ChangeTracker.Entries<IHaveCompany>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CompanyId == Guid.Empty)
            {
                if (CurrentUser.CompanyId.HasValue)
                {
                    entry.Entity.CompanyId = CurrentUser.CompanyId.Value;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Entity '{entry.Entity.GetType().Name}' requires a tenant CompanyId.");
                }
            }

            if (entry.State == EntityState.Added &&
                CurrentUser.CompanyId.HasValue &&
                entry.Entity.CompanyId != Guid.Empty &&
                entry.Entity.CompanyId != CurrentUser.CompanyId.Value)
            {
                throw new InvalidOperationException(
                    $"Entity '{entry.Entity.GetType().Name}' cannot be created for a different tenant.");
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CompanyId).IsModified = false;
            }
        }
    }
}
