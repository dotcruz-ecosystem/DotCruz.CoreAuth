using DotCruz.CoreAuth.Domain.Entities.Base;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.Shared.Security.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DotCruz.CoreAuth.Infrastructure.Data;

public class CoreAuthDbContext(
    DbContextOptions<CoreAuthDbContext> options,
    ITenantProvider tenantProvider) : DbContext(options)
{
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ActivationToken> ActivationTokens { get; set; }

    public Guid TenantIdValue => tenantProvider.TenantId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreAuthDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => e.ClrType != null))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            Expression? combinedExpr = null;

            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var propertyDeletedAt = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var nullValue = Expression.Constant(null, typeof(DateTime?));
                combinedExpr = Expression.Equal(propertyDeletedAt, nullValue);
            }

            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var propertyTenantId = Expression.Property(parameter, nameof(TenantEntity.TenantId));
                var providerTenantId = Expression.Property(Expression.Constant(this), nameof(TenantIdValue));
                var tenantFilterExpr = Expression.Equal(propertyTenantId, providerTenantId);

                combinedExpr = combinedExpr != null 
                    ? Expression.AndAlso(combinedExpr, tenantFilterExpr) 
                    : tenantFilterExpr;
            }

            if (combinedExpr != null)
            {
                var lambda = Expression.Lambda(combinedExpr, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntity baseEntity && entry.State == EntityState.Modified)
            {
                baseEntity.Touch();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
