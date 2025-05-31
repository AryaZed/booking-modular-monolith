using System.Linq.Expressions;
using Identity.Domain.Aggregates.User;

namespace Identity.Application.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApplicationUser> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationUser>> GetUsersInTenantAsync(long tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default);
    Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default);
    Task<UserTenantRole> AddUserToTenantWithRoleAsync(ApplicationUser user, long tenantId, long roleId, bool isDefault = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserTenantRole>> GetUserTenantRolesAsync(long userId, CancellationToken cancellationToken = default);
} 