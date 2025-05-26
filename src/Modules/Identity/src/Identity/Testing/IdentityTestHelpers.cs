using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BuildingBlocks.Constants;
using Identity.Identity.Models;

namespace Identity.Testing;

public static class IdentityTestHelpers
{
    /// <summary>
    /// Creates a test user with the specified claims for unit testing
    /// </summary>
    public static ClaimsPrincipal CreateTestUser(
        long userId, 
        string email, 
        string username = null,
        string roleName = null,
        long? tenantId = null,
        TenantType? tenantType = null,
        IEnumerable<string> permissions = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
        };
        
        if (!string.IsNullOrEmpty(username))
            claims.Add(new Claim(ClaimTypes.Name, username));
            
        if (!string.IsNullOrEmpty(roleName))
            claims.Add(new Claim(ClaimTypes.Role, roleName));
            
        if (tenantId.HasValue)
            claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantId, tenantId.Value.ToString()));
            
        if (tenantType.HasValue)
            claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantType, tenantType.Value.ToString()));
        
        if (permissions != null)
        {
            claims.AddRange(permissions.Select(p => new Claim(IdentityConstant.ClaimTypes.Permission, p)));
        }
        
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
    
    /// <summary>
    /// Creates a mock ApplicationUser for testing
    /// </summary>
    public static ApplicationUser CreateTestApplicationUser(
        long userId,
        string email,
        string firstName = "Test",
        string lastName = "User")
    {
        return new ApplicationUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };
    }
    
    /// <summary>
    /// Creates a test tenant for unit testing
    /// </summary>
    public static Tenant CreateTestTenant(
        long tenantId,
        string name,
        TenantType tenantType = TenantType.Brand,
        long? parentTenantId = null)
    {
        return Tenant.Create(name, tenantType, parentTenantId, $"Test {tenantType} {name}", null);
    }
} 