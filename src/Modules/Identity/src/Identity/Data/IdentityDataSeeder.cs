using BuildingBlocks.Domain;
using BuildingBlocks.Constants;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Identity.Data;

public class IdentityDataSeeder : IDataSeeder
{
    private readonly IdentityContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<IdentityDataSeeder> _logger;
    private readonly IOptions<AppOptions> _appOptions;

    public IdentityDataSeeder(
        IdentityContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<IdentityDataSeeder> logger,
        IOptions<AppOptions> appOptions)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _appOptions = appOptions;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedModulesAsync();
            await SeedSystemTenantAsync();
            await SeedSystemRolesAsync();
            await SeedSystemAdminUserAsync();
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Seed data completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding data");
            throw;
        }
    }

    private async Task SeedModulesAsync()
    {
        if (await _dbContext.Modules.AnyAsync())
            return;

        var modules = new List<Module>
        {
            new() { Name = "Identity", Description = "Core identity and authentication services", IsRequired = true, IsEnabled = true },
            new() { Name = "Booking", Description = "Booking management", IsRequired = false, IsEnabled = true },
            new() { Name = "Flight", Description = "Flight management", IsRequired = false, IsEnabled = true },
            new() { Name = "Passenger", Description = "Passenger management", IsRequired = false, IsEnabled = true }
        };

        await _dbContext.Modules.AddRangeAsync(modules);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedSystemTenantAsync()
    {
        if (await _dbContext.Tenants.AnyAsync())
            return;

        var systemTenant = new Tenant
        {
            Name = "System",
            DisplayName = "System Administration",
            TenantType = IdentityConstant.TenantType.System,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Tenants.AddAsync(systemTenant);
        await _dbContext.SaveChangesAsync();

        // Add module licenses to system tenant
        var modules = await _dbContext.Modules.ToListAsync();
        foreach (var module in modules)
        {
            await _dbContext.TenantModules.AddAsync(new TenantModule
            {
                TenantId = systemTenant.Id,
                ModuleId = module.Id,
                IsEnabled = true,
                ExpiresAt = DateTime.UtcNow.AddYears(10) // Long expiration for system tenant
            });
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedSystemRolesAsync()
    {
        if (await _roleManager.Roles.AnyAsync())
            return;

        // Create system roles
        var systemAdminRole = new ApplicationRole
        {
            Name = IdentityConstant.Role.SystemAdmin,
            NormalizedName = IdentityConstant.Role.SystemAdmin.ToUpperInvariant(),
            Description = "System administrator with full access to all features",
            TenantId = null, // System-wide role
            IsSystemDefault = true
        };

        var customerRole = new ApplicationRole
        {
            Name = IdentityConstant.Role.Customer,
            NormalizedName = IdentityConstant.Role.Customer.ToUpperInvariant(),
            Description = "Customer with limited access to booking features",
            TenantId = null, // System-wide role
            IsSystemDefault = true
        };

        var brandAdminRole = new ApplicationRole
        {
            Name = IdentityConstant.Role.BrandAdmin,
            NormalizedName = IdentityConstant.Role.BrandAdmin.ToUpperInvariant(),
            Description = "Brand administrator with access to brand management features",
            TenantId = null, // System-wide role
            IsSystemDefault = true
        };

        var branchAdminRole = new ApplicationRole
        {
            Name = IdentityConstant.Role.BranchAdmin,
            NormalizedName = IdentityConstant.Role.BranchAdmin.ToUpperInvariant(),
            Description = "Branch administrator with access to branch management features",
            TenantId = null, // System-wide role
            IsSystemDefault = true
        };

        await _roleManager.CreateAsync(systemAdminRole);
        await _roleManager.CreateAsync(customerRole);
        await _roleManager.CreateAsync(brandAdminRole);
        await _roleManager.CreateAsync(branchAdminRole);

        // Add permissions to the roles
        await AddPermissionsToRoleAsync(systemAdminRole, PermissionsConstant.SystemAdminPermissions);
        await AddPermissionsToRoleAsync(brandAdminRole, PermissionsConstant.BrandAdminPermissions);
        await AddPermissionsToRoleAsync(branchAdminRole, PermissionsConstant.BranchManagerPermissions);
        await AddPermissionsToRoleAsync(customerRole, PermissionsConstant.RegularUserPermissions);
    }

    private async Task AddPermissionsToRoleAsync(ApplicationRole role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            await _dbContext.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission
            });
        }
    }

    private async Task SeedSystemAdminUserAsync()
    {
        const string adminEmail = "admin@system.com";
        
        if (await _userManager.FindByEmailAsync(adminEmail) != null)
            return;

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            PhoneNumber = "+1234567890",
            PhoneNumberConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createUserResult = await _userManager.CreateAsync(adminUser, "Admin123!");
        if (!createUserResult.Succeeded)
        {
            var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create admin user: {errors}");
        }

        // Get the system tenant
        var systemTenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.TenantType == IdentityConstant.TenantType.System);
        if (systemTenant == null)
            throw new Exception("System tenant not found");

        // Get the system admin role
        var systemAdminRole = await _roleManager.FindByNameAsync(IdentityConstant.Role.SystemAdmin);
        if (systemAdminRole == null)
            throw new Exception("System admin role not found");

        // Assign user to system tenant with admin role
        await _dbContext.UserTenantRoles.AddAsync(new UserTenantRole
        {
            UserId = adminUser.Id,
            TenantId = systemTenant.Id,
            RoleId = systemAdminRole.Id,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }
} 