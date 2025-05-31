using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identity.Identity.Models;

namespace Identity.Identity.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ApplicationUser> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApplicationUser> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<ApplicationUser>> GetUsersInTenantAsync(long tenantId, CancellationToken cancellationToken = default);
        Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 