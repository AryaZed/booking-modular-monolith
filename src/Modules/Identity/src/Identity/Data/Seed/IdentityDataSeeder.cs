using BuildingBlocks.EFCore;
using Identity.Identity.Constants;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Data;

public class IdentityDataSeeder : IDataSeeder
{
    private readonly RoleManager<IdentityRole<long>> _roleManager;
    private readonly IdentityContext _identityContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<long>> roleManager,
        IdentityContext identityContext,
        ILogger<IdentityDataSeeder> logger
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _identityContext = identityContext;
        _logger = logger;
    }

    public async Task SeedAllAsync()
    {
        if (await _identityContext.Database.CanConnectAsync())
        {
            await SeedRoles();
            await SeedUsers();
            await SeedTenantsAsync();
            await SeedModulesAsync();
            await SeedTenantModulesAsync();
        }
    }

    private async Task SeedRoles()
    {
        if (!await _identityContext.Roles.AnyAsync())
        {
            if (await _roleManager.RoleExistsAsync(Constants.Role.Admin) == false)
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(Constants.Role.Admin));
                await _identityContext.SaveChangesAsync();
            }

            if (await _roleManager.RoleExistsAsync(Constants.Role.User) == false)
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(Constants.Role.User));
                await _identityContext.SaveChangesAsync();
            }
        }
    }

    private async Task SeedUsers()
    {
        if (!await _identityContext.Users.AnyAsync())
        {
            if (await _userManager.FindByNameAsync("meysamh") == null)
            {
                var user = new ApplicationUser
                           {
                               FirstName = "Meysam",
                               LastName = "Hadeli",
                               UserName = "meysamh",
                               Email = "meysam@test.com",
                               SecurityStamp = Guid.NewGuid().ToString(),
                               PassPortNumber = String.Empty
                           };

                var result = await _userManager.CreateAsync(user, "Admin@123456");

                if (result.Succeeded) await _userManager.AddToRoleAsync(user, Constants.Role.Admin);

                await _identityContext.SaveChangesAsync();
            }

            if (await _userManager.FindByNameAsync("meysamh2") == null)
            {
                var user = new ApplicationUser
                           {
                               FirstName = "Meysam",
                               LastName = "Hadeli",
                               UserName = "meysamh2",
                               Email = "meysam2@test.com",
                               SecurityStamp = Guid.NewGuid().ToString(),
                               PassPortNumber = String.Empty
                           };

                var result = await _userManager.CreateAsync(user, "User@123456");

                if (result.Succeeded) await _userManager.AddToRoleAsync(user, Constants.Role.User);

                await _identityContext.SaveChangesAsync();
            }
        }
    }
    
    private async Task SeedTenantsAsync()
    {
        if (!await _identityContext.Tenants.AnyAsync())
        {
            // Create system tenant
            var systemTenant = Tenant.Create("System", TenantType.System, null, "System tenant", null);
            
            // Create a demo brand tenant
            var brandTenant = Tenant.Create("Demo Brand", TenantType.Brand, null, "Demo brand tenant", null);
            
            // Create a demo branch tenant
            var branchTenant = Tenant.Create("Demo Branch", TenantType.Branch, null, "Demo branch tenant", null);
            
            await _identityContext.Tenants.AddRangeAsync(systemTenant, brandTenant, branchTenant);
            await _identityContext.SaveChangesAsync();
            
            _logger.LogInformation("Seeded tenants: System, Demo Brand, Demo Branch");
        }
    }
    
    private async Task SeedModulesAsync()
    {
        if (!await _identityContext.Modules.AnyAsync())
        {
            var modules = new List<Module>
            {
                Module.Create("Identity", "identity", "User identity and authentication module"),
                Module.Create("Booking", "booking", "Booking management module"),
                Module.Create("Flight", "flight", "Flight management module"),
                Module.Create("Passenger", "passenger", "Passenger management module"),
                Module.Create("Payment", "payment", "Payment processing module"),
                Module.Create("Notification", "notification", "Notification service module"),
                Module.Create("Analytics", "analytics", "Analytics and reporting module")
            };
            
            await _identityContext.Modules.AddRangeAsync(modules);
            await _identityContext.SaveChangesAsync();
            
            _logger.LogInformation("Seeded {Count} modules", modules.Count);
        }
    }
    
    private async Task SeedTenantModulesAsync()
    {
        var systemTenant = await _identityContext.Tenants.FirstOrDefaultAsync(t => t.Type == TenantType.System);
        var brandTenant = await _identityContext.Tenants.FirstOrDefaultAsync(t => t.Type == TenantType.Brand);
        var branchTenant = await _identityContext.Tenants.FirstOrDefaultAsync(t => t.Type == TenantType.Branch);
        
        if (systemTenant != null)
        {
            // System tenant gets access to all modules
            var allModules = await _identityContext.Modules.ToListAsync();
            var existingTenantModules = await _identityContext.TenantModules
                .Where(tm => tm.TenantId == systemTenant.Id)
                .Select(tm => tm.ModuleId)
                .ToListAsync();
                
            var modulesToAdd = allModules
                .Where(m => !existingTenantModules.Contains(m.Id))
                .Select(m => TenantModule.Create(systemTenant.Id, m.Id))
                .ToList();
                
            if (modulesToAdd.Any())
            {
                await _identityContext.TenantModules.AddRangeAsync(modulesToAdd);
                await _identityContext.SaveChangesAsync();
                
                _logger.LogInformation("Seeded {Count} module subscriptions for system tenant", modulesToAdd.Count);
            }
        }
        
        if (brandTenant != null && !await _identityContext.TenantModules.AnyAsync(tm => tm.TenantId == brandTenant.Id))
        {
            // Demo brand gets access to core modules
            var coreModules = await _identityContext.Modules
                .Where(m => m.Code == "identity" || m.Code == "booking" || m.Code == "notification")
                .ToListAsync();
                
            var brandModules = coreModules
                .Select(m => TenantModule.Create(brandTenant.Id, m.Id))
                .ToList();
                
            if (brandModules.Any())
            {
                await _identityContext.TenantModules.AddRangeAsync(brandModules);
                await _identityContext.SaveChangesAsync();
                
                _logger.LogInformation("Seeded {Count} module subscriptions for demo brand tenant", brandModules.Count);
            }
        }
        
        if (branchTenant != null && !await _identityContext.TenantModules.AnyAsync(tm => tm.TenantId == branchTenant.Id))
        {
            // Demo branch gets access to even fewer modules
            var branchModules = await _identityContext.Modules
                .Where(m => m.Code == "identity" || m.Code == "booking")
                .ToListAsync();
                
            var tenantModules = branchModules
                .Select(m => TenantModule.Create(branchTenant.Id, m.Id))
                .ToList();
                
            if (tenantModules.Any())
            {
                await _identityContext.TenantModules.AddRangeAsync(tenantModules);
                await _identityContext.SaveChangesAsync();
                
                _logger.LogInformation("Seeded {Count} module subscriptions for demo branch tenant", tenantModules.Count);
            }
        }
    }
}
