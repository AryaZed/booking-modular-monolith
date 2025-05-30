using BuildingBlocks.EFCore;
using Identity.Identity.Constants;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Identity.Data.Seed;

public class IdentityDataSeeder : IDataSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityContext _identityContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IdentityContext identityContext
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _identityContext = identityContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _identityContext.Database.CanConnectAsync())
        {
            await SeedRoles();
            await SeedUsers();
        }
    }

    private async Task SeedRoles()
    {
        if (!await _identityContext.Roles.AnyAsync())
        {
            if (await _roleManager.RoleExistsAsync(Constants.Role.Admin) == false)
            {
                await _roleManager.CreateAsync(ApplicationRole.Create(
                    name: Constants.Role.Admin,
                    description: "Administrator role with full permissions",
                    isDefault: true
                ));
                await _identityContext.SaveChangesAsync();
            }

            if (await _roleManager.RoleExistsAsync(Constants.Role.User) == false)
            {
                await _roleManager.CreateAsync(ApplicationRole.Create(
                    name: Constants.Role.User,
                    description: "Regular user role with limited permissions",
                    isDefault: true
                ));
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
                var user = ApplicationUser.Create(
                    email: "meysam@test.com",
                    firstName: "Meysam",
                    lastName: "Hadeli"
                );
                
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.UserName = "meysamh";
                
                user.SetPassportNumber(string.Empty);

                var result = await _userManager.CreateAsync(user, "Admin@123456");

                if (result.Succeeded) await _userManager.AddToRoleAsync(user, Constants.Role.Admin);

                await _identityContext.SaveChangesAsync();
            }

            if (await _userManager.FindByNameAsync("meysamh2") == null)
            {
                var user = ApplicationUser.Create(
                    email: "meysam2@test.com",
                    firstName: "Meysam",
                    lastName: "Hadeli"
                );
                
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.UserName = "meysamh2";
                
                user.SetPassportNumber(string.Empty);

                var result = await _userManager.CreateAsync(user, "User@123456");

                if (result.Succeeded) await _userManager.AddToRoleAsync(user, Constants.Role.User);

                await _identityContext.SaveChangesAsync();
            }
        }
    }
}
