using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Identity.Identity.Constants;
using IdentityModel;

namespace BookingMonolith.Identity.Configurations;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };


    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            // Standard booking scope
            new(Constants.StandardScopes.Booking),
            
            // System-level scopes
            new("system.admin", "Full system administration"),
            new("system.support", "System support team access"),
            
            // Brand-level scopes
            new("brand.admin", "Brand administration"),
            new("brand.manager", "Brand management"),
            
            // Branch-level scopes
            new("branch.admin", "Branch administration"),
            new("branch.staff", "Branch staff access"),
            
            // Customer-level scopes
            new("customer", "Customer access"),
            
            // Third-party API scopes
            new("api.read", "API read access"),
            new("api.write", "API write access"),
            
            // Role claims
            new(JwtClaimTypes.Role, new List<string> {"role"})
        };


    public static IList<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new(Constants.StandardScopes.Booking)
            {
                Scopes = { 
                    Constants.StandardScopes.Booking,
                    "system.admin", "system.support",
                    "brand.admin", "brand.manager",
                    "branch.admin", "branch.staff",
                    "customer",
                    "api.read", "api.write"
                }
            },
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // System Admin Client
            new()
            {
                ClientId = "system_admin_client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("admin_secret".Sha256()) },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    JwtClaimTypes.Role,
                    Constants.StandardScopes.Booking,
                    "system.admin", "brand.admin", "branch.admin", "customer",
                    "api.read", "api.write"
                },
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true
            },
            
            // Brand Admin Client
            new()
            {
                ClientId = "brand_admin_client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("brand_secret".Sha256()) },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    JwtClaimTypes.Role,
                    Constants.StandardScopes.Booking,
                    "brand.admin", "branch.admin", "customer",
                    "api.read"
                },
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true
            },
            
            // Branch Admin Client
            new()
            {
                ClientId = "branch_admin_client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("branch_secret".Sha256()) },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    JwtClaimTypes.Role,
                    Constants.StandardScopes.Booking,
                    "branch.admin", "customer",
                    "api.read"
                },
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true
            },
            
            // Customer Client
            new()
            {
                ClientId = "customer_client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("customer_secret".Sha256()) },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    JwtClaimTypes.Role,
                    Constants.StandardScopes.Booking,
                    "customer"
                },
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true
            },
            
            // Original client (keep for backward compatibility)
            new()
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    JwtClaimTypes.Role,
                    Constants.StandardScopes.Booking,
                },
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 3600,
                AlwaysIncludeUserClaimsInIdToken = true
            }
        };
}
