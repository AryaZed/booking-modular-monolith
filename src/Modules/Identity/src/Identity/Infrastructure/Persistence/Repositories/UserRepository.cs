using System.Linq.Expressions;
using Identity.Application.Interfaces;
using Identity.Domain.Aggregates.Role;
using Identity.Domain.Aggregates.Tenant;
using Identity.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityContext _context;

    public UserRepository(IdentityContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Tenant)
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<ApplicationUser> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Tenant)
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant(), cancellationToken);
    }

    public async Task<ApplicationUser> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Tenant)
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Role)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == userName.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersInTenantAsync(long tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenantRoles
            .Where(utr => utr.TenantId == tenantId)
            .Select(utr => utr.User)
            .Distinct()
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(predicate)
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Tenant)
            .Include(u => u.TenantRoles)
                .ThenInclude(tr => tr.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        // Implement as soft delete
        user.SoftDelete();
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(predicate, cancellationToken);
    }

    public async Task<UserTenantRole> AddUserToTenantWithRoleAsync(ApplicationUser user, long tenantId, long roleId, bool isDefault = false, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
        var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);

        if (tenant == null || role == null)
        {
            throw new InvalidOperationException("Tenant or role not found");
        }

        var userTenantRole = UserTenantRole.Create(user, tenant, role, isDefault);
        await _context.UserTenantRoles.AddAsync(userTenantRole, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return userTenantRole;
    }

    public async Task<IEnumerable<UserTenantRole>> GetUserTenantRolesAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenantRoles
            .Where(utr => utr.UserId == userId)
            .Include(utr => utr.Tenant)
            .Include(utr => utr.Role)
            .ToListAsync(cancellationToken);
    }
} 