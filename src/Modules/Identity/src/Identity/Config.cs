using BuildingBlocks.Constants;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Identity.Identity.Constants;
using IdentityModel;

namespace Identity;

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
            new($"{IdentityConstant.TenantType.System.ToLower()}.admin", "Full system administration"),
            new($"{IdentityConstant.TenantType.System.ToLower()}.support", "System support team access"),
            
            // Brand-level scopes
            new($"{IdentityConstant.TenantType.Brand.ToLower()}.admin", $"{IdentityConstant.TenantType.Brand} administration"),
            new($"{IdentityConstant.TenantType.Brand.ToLower()}.manager", $"{IdentityConstant.TenantType.Brand} management"),
            new($"{IdentityConstant.TenantType.Brand.ToLower()}.user", $"{IdentityConstant.TenantType.Brand} user access"),
            
            // Branch-level scopes
            new($"{IdentityConstant.TenantType.Branch.ToLower()}.admin", $"{IdentityConstant.TenantType.Branch} administration"),
            new($"{IdentityConstant.TenantType.Branch.ToLower()}.manager", $"{IdentityConstant.TenantType.Branch} management"),
            new($"{IdentityConstant.TenantType.Branch.ToLower()}.cashier", $"{IdentityConstant.TenantType.Branch} cashier access"),
            new($"{IdentityConstant.TenantType.Branch.ToLower()}.host", $"{IdentityConstant.TenantType.Branch} host access"),
            new($"{IdentityConstant.TenantType.Branch.ToLower()}.staff", $"{IdentityConstant.TenantType.Branch} general staff access"),
            
            // Department-level scopes (if needed)
            new($"{IdentityConstant.TenantType.Department.ToLower()}.admin", $"{IdentityConstant.TenantType.Department} administration"),
            new($"{IdentityConstant.TenantType.Department.ToLower()}.manager", $"{IdentityConstant.TenantType.Department} management"),
            
            // Customer-level scopes
            new($"{IdentityConstant.Role.Customer.ToLower()}", $"{IdentityConstant.Role.Customer} access"),
            
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
                    
                    // System-level scopes
                    $"{IdentityConstant.TenantType.System.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.System.ToLower()}.support",
                    
                    // Brand-level scopes
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.user",
                    
                    // Branch-level scopes
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.cashier",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.host",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.staff",
                    
                    // Department-level scopes
                    $"{IdentityConstant.TenantType.Department.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Department.ToLower()}.manager",
                    
                    // Customer-level scopes
                    IdentityConstant.Role.Customer.ToLower(),
                    
                    // API scopes
                    "api.read", "api.write"
                }
            },
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // System Admin Client
            CreateClient(
                "system_admin_client",
                "admin_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.System.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Department.ToLower()}.admin",
                    IdentityConstant.Role.Customer.ToLower(),
                    "api.read", "api.write"
                }),
                
            // System Support Client
            CreateClient(
                "system_support_client",
                "support_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.System.ToLower()}.support",
                    "api.read"
                }),
            
            // Brand Admin Client
            CreateClient(
                "brand_admin_client",
                "brand_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Department.ToLower()}.admin",
                    IdentityConstant.Role.Customer.ToLower(),
                    "api.read"
                }),
                
            // Brand Manager Client
            CreateClient(
                "brand_manager_client",
                "brand_mgr_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.manager",
                    "api.read"
                }),
                
            // Brand User Client
            CreateClient(
                "brand_user_client",
                "brand_user_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Brand.ToLower()}.user",
                    "api.read"
                }),
            
            // Branch Admin Client
            CreateClient(
                "branch_admin_client",
                "branch_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.admin",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.cashier",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.host",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.staff",
                    $"{IdentityConstant.TenantType.Department.ToLower()}.admin",
                    IdentityConstant.Role.Customer.ToLower(),
                    "api.read"
                }),
                
            // Branch Manager Client
            CreateClient(
                "branch_manager_client",
                "branch_mgr_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.manager",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.cashier",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.host",
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.staff",
                    "api.read"
                }),
                
            // Branch Cashier Client
            CreateClient(
                "branch_cashier_client",
                "cashier_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.cashier",
                    "api.read"
                }),
                
            // Branch Host Client
            CreateClient(
                "branch_host_client",
                "host_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.host",
                    "api.read"
                }),
                
            // Branch Staff Client
            CreateClient(
                "branch_staff_client",
                "staff_secret",
                new[]
                {
                    $"{IdentityConstant.TenantType.Branch.ToLower()}.staff",
                    "api.read"
                }),
            
            // Customer Client
            CreateClient(
                "customer_client",
                "customer_secret",
                new[]
                {
                    IdentityConstant.Role.Customer.ToLower()
                }),
            
            // Original client (keep for backward compatibility)
            CreateClient(
                "client",
                "secret",
                new string[] { })
        };

    private static Client CreateClient(string clientId, string secret, string[] additionalScopes)
    {
        var baseScopes = new[]
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            JwtClaimTypes.Role,
            Constants.StandardScopes.Booking
        };

        var allScopes = baseScopes.Concat(additionalScopes).ToArray();

        return new Client
        {
            ClientId = clientId,
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            ClientSecrets = { new Secret(secret.Sha256()) },
            AllowedScopes = allScopes,
            AccessTokenLifetime = 3600,
            RefreshTokenUsage = TokenUsage.ReUse,
            RefreshTokenExpiration = TokenExpiration.Absolute,
            AbsoluteRefreshTokenLifetime = IdentityConstant.Auth.RefreshTokenExpirationDays * 86400, // Days to seconds
            AllowOfflineAccess = true
        };
    }
}
