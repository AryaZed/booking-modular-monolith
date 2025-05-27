using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain.Event;
using BuildingBlocks.Domain.Model;
using BuildingBlocks.EFCore;
using Identity.Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Identity.Data;

public sealed class IdentityContext(
    DbContextOptions<IdentityContext> options,
    IHttpContextAccessor httpContextAccessor,
    ICurrentTenantProvider currentTenantProvider = null) : IdentityDbContext<ApplicationUser, ApplicationRole, long,
    IdentityUserClaim<long>,
    IdentityUserRole<long>, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>(options), IDbContext
{
    private IDbContextTransaction _currentTransaction;
    private readonly ICurrentTenantProvider _currentTenantProvider = currentTenantProvider;

    // New DbSets for our custom models
    public DbSet<UserTenantRole> UserTenantRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OneTimePassword> OneTimePasswords { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure UserTenantRole relationships
        builder.Entity<UserTenantRole>()
            .HasOne(utr => utr.User)
            .WithMany(u => u.TenantRoles)
            .HasForeignKey(utr => utr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserTenantRole>()
            .HasOne(utr => utr.Role)
            .WithMany(r => r.UserTenantRoles)
            .HasForeignKey(utr => utr.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserTenantRole>()
            .HasOne(utr => utr.Tenant)
            .WithMany(t => t.UserRoles)
            .HasForeignKey(utr => utr.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure RolePermission relationships
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Tenant hierarchical relationship
        builder.Entity<Tenant>()
            .HasOne(t => t.ParentTenant)
            .WithMany(t => t.ChildTenants)
            .HasForeignKey(t => t.ParentTenantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure RefreshToken relationship
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure global query filters

        // Soft delete filters
        builder.Entity<ApplicationUser>()
            .HasQueryFilter(u => !u.IsDeleted);

        builder.Entity<ApplicationRole>()
            .HasQueryFilter(r => !r.IsDeleted);

        builder.Entity<Tenant>()
            .HasQueryFilter(t => !t.IsDeleted);

        // Multi-tenant filters
        if (_currentTenantProvider != null)
        {
            // For UserTenantRole, filter by current tenant if tenant context is set
            builder.Entity<UserTenantRole>()
                .HasQueryFilter(utr => _currentTenantProvider.TenantId == null ||
                               utr.TenantId == _currentTenantProvider.TenantId);

            // For ApplicationRole, filter tenant-specific roles by current tenant
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(r => !r.IsDeleted &&
                               (r.TenantId == null || _currentTenantProvider.TenantId == null ||
                                r.TenantId == _currentTenantProvider.TenantId));

            // For RolePermission, include system permissions and tenant-specific permissions
            builder.Entity<RolePermission>()
                .HasQueryFilter(rp => _currentTenantProvider.TenantId == null ||
                               rp.Role.TenantId == null ||
                               rp.Role.TenantId == _currentTenantProvider.TenantId);
        }

        base.OnModelCreating(builder);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    //ref: https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency#execution-strategies-and-transactions
    public Task ExecuteTransactionalAsync(CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            try
            {
                await SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public IExecutionStrategy CreateExecutionStrategy() => Database.CreateExecutionStrategy();

    public IReadOnlyList<IDomainEvent> GetDomainEvents()
    {
        var domainEntities = ChangeTracker
            .Entries<Aggregate>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        if (!domainEntities.Any())
            return new List<IDomainEvent>();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList()
            .AsReadOnly();

        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        return domainEvents;
    }
}
