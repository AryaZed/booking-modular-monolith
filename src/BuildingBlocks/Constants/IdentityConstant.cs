namespace BuildingBlocks.Constants;

public static class IdentityConstant
{
    /// <summary>
    /// Standard role names for the application
    /// </summary>
    public static class Role
    {
        public const string Admin = "admin";
        public const string User = "user";
        public const string SystemAdmin = "system_admin";
        public const string BrandAdmin = "brand_admin";
        public const string BranchAdmin = "branch_admin";
        public const string Customer = "customer";
    }

    /// <summary>
    /// Tenant types in the hierarchical structure
    /// </summary>
    public static class TenantType
    {
        public const string System = "System";
        public const string Brand = "Brand";
        public const string Branch = "Branch";
        public const string Department = "Department";
        public const string Team = "Team";
        public const string Project = "Project";
        public const string Custom = "Custom";
    }

    /// <summary>
    /// Custom claim types used in JWT tokens
    /// </summary>
    public static class ClaimTypes
    {
        public const string TenantId = "tenant_id";
        public const string TenantType = "tenant_type";
        public const string Permission = "permission";
        public const string UserId = "user_id";
    }

    /// <summary>
    /// Authentication-related constants
    /// </summary>
    public static class Auth
    {
        public const string DefaultScheme = "Bearer";
        public const int DefaultExpirationMinutes = 60;
        public const int RefreshTokenExpirationDays = 7;
    }

    /// <summary>
    /// Password policy constants
    /// </summary>
    public static class Password
    {
        public const int MinimumLength = 8;
        public const bool RequireDigit = true;
        public const bool RequireLowercase = true;
        public const bool RequireUppercase = true;
        public const bool RequireNonAlphanumeric = true;
        public const int RequiredUniqueChars = 1;
    }
}
