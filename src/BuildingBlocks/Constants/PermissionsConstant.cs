using System.Collections.Generic;

namespace BuildingBlocks.Constants;

public static class PermissionsConstant
{
    public static class Identity
    {
        public const string View = "Identity.View";
        public const string Create = "Identity.Create";
        public const string Edit = "Identity.Edit";
        public const string Delete = "Identity.Delete";
        
        public static class Users
        {
            public const string View = "Identity.Users.View";
            public const string Create = "Identity.Users.Create";
            public const string Edit = "Identity.Users.Edit";
            public const string Delete = "Identity.Users.Delete";
            public const string ChangePassword = "Identity.Users.ChangePassword";
            public const string ResetPassword = "Identity.Users.ResetPassword";
            public const string ManageRoles = "Identity.Users.ManageRoles";
            public const string ViewPersonalData = "Identity.Users.ViewPersonalData";
        }
        
        public static class Roles
        {
            public const string View = "Identity.Roles.View";
            public const string Create = "Identity.Roles.Create";
            public const string Edit = "Identity.Roles.Edit";
            public const string Delete = "Identity.Roles.Delete";
            public const string ManagePermissions = "Identity.Roles.ManagePermissions";
        }
        
        public static class Tenants
        {
            public const string View = "Identity.Tenants.View";
            public const string Create = "Identity.Tenants.Create";
            public const string Edit = "Identity.Tenants.Edit";
            public const string Delete = "Identity.Tenants.Delete";
            public const string ManageUsers = "Identity.Tenants.ManageUsers";
            public const string ManageRoles = "Identity.Tenants.ManageRoles";
            public const string ManageSettings = "Identity.Tenants.ManageSettings";
        }
    }

    public static class Customers
    {
        public const string View = "Customers.View";
        public const string Create = "Customers.Create";
        public const string Edit = "Customers.Edit";
        public const string Delete = "Customers.Delete";
        public const string ManageAccounts = "Customers.ManageAccounts";
        public const string ViewHistory = "Customers.ViewHistory";
    }
    
    public static class Branches
    {
        public const string View = "Branches.View";
        public const string Create = "Branches.Create";
        public const string Edit = "Branches.Edit";
        public const string Delete = "Branches.Delete";
        public const string ManageCustomers = "Branches.ManageCustomers";
        public const string ManageBranchUsers = "Branches.ManageBranchUsers";
        public const string ManageSettings = "Branches.ManageSettings";
        public const string ViewReports = "Branches.ViewReports";
    }
    
    public static class Brands
    {
        public const string View = "Brands.View";
        public const string Create = "Brands.Create";
        public const string Edit = "Brands.Edit";
        public const string Delete = "Brands.Delete";
        public const string ManageBranches = "Brands.ManageBranches";
        public const string ManageBrandUsers = "Brands.ManageBrandUsers";
        public const string ManageSettings = "Brands.ManageSettings";
        public const string ViewReports = "Brands.ViewReports";
    }
    
    public static class System
    {
        public const string ManageRoles = "System.ManageRoles";
        public const string ManageBrands = "System.ManageBrands";
        public const string ViewAuditLogs = "System.ViewAuditLogs";
        public const string ManageUsers = "System.ManageUsers";
        public const string ManageSettings = "System.ManageSettings";
        public const string ViewDashboard = "System.ViewDashboard";
        public const string ManageMaintenance = "System.ManageMaintenance";
        public const string ExecuteDiagnostics = "System.ExecuteDiagnostics";
    }
    
    public static class RoleManagement
    {
        public const string CreateRole = "RoleManagement.CreateRole";
        public const string EditRole = "RoleManagement.EditRole";
        public const string DeleteRole = "RoleManagement.DeleteRole";
        public const string AssignRole = "RoleManagement.AssignRole";
        public const string ManagePermissions = "RoleManagement.ManagePermissions";
        public const string ViewRoleAudit = "RoleManagement.ViewRoleAudit";
    }
    
    public static class Booking
    {
        public const string View = "Booking.View";
        public const string Create = "Booking.Create";
        public const string Edit = "Booking.Edit";
        public const string Delete = "Booking.Delete";
        public const string Cancel = "Booking.Cancel";
        public const string Approve = "Booking.Approve";
        public const string Reject = "Booking.Reject";
        public const string ManagePayment = "Booking.ManagePayment";
        public const string ViewHistory = "Booking.ViewHistory";
        public const string ExportData = "Booking.ExportData";
    }
    
    public static class Flight
    {
        public const string View = "Flight.View";
        public const string Create = "Flight.Create";
        public const string Edit = "Flight.Edit";
        public const string Delete = "Flight.Delete";
        public const string ManageSchedule = "Flight.ManageSchedule";
        public const string ManageSeats = "Flight.ManageSeats";
        public const string ManagePricing = "Flight.ManagePricing";
        public const string ViewReports = "Flight.ViewReports";
    }
    
    // Predefined permission sets for different roles
    
    // System Administrator permission set
    public static readonly HashSet<string> SystemAdminPermissions = new()
    {
        // Identity permissions
        Identity.View, Identity.Create, Identity.Edit, Identity.Delete,
        Identity.Users.View, Identity.Users.Create, Identity.Users.Edit, Identity.Users.Delete,
        Identity.Users.ChangePassword, Identity.Users.ResetPassword, Identity.Users.ManageRoles,
        Identity.Roles.View, Identity.Roles.Create, Identity.Roles.Edit, Identity.Roles.Delete,
        Identity.Roles.ManagePermissions,
        Identity.Tenants.View, Identity.Tenants.Create, Identity.Tenants.Edit, Identity.Tenants.Delete,
        Identity.Tenants.ManageUsers, Identity.Tenants.ManageRoles, Identity.Tenants.ManageSettings,
        
        // System permissions
        System.ManageRoles, System.ManageBrands, System.ViewAuditLogs, System.ManageUsers,
        System.ManageSettings, System.ViewDashboard, System.ManageMaintenance, System.ExecuteDiagnostics,
        
        // Brand permissions
        Brands.View, Brands.Create, Brands.Edit, Brands.Delete,
        Brands.ManageBranches, Brands.ManageBrandUsers, Brands.ManageSettings, Brands.ViewReports
    };
    
    // Brand Administrator permission set
    public static readonly HashSet<string> BrandAdminPermissions = new()
    {
        // Brand permissions
        Brands.View, Brands.Edit, Brands.ManageBranches, Brands.ManageBrandUsers,
        Brands.ManageSettings, Brands.ViewReports,
        
        // Branch permissions
        Branches.View, Branches.Create, Branches.Edit, Branches.Delete,
        Branches.ManageCustomers, Branches.ManageBranchUsers, Branches.ManageSettings, Branches.ViewReports,
        
        // Customer permissions
        Customers.View, Customers.Create, Customers.Edit, Customers.Delete,
        Customers.ManageAccounts, Customers.ViewHistory,
        
        // Limited identity permissions
        Identity.Users.View, Identity.Users.Create, Identity.Users.Edit,
        Identity.Users.ChangePassword, Identity.Users.ManageRoles,
        Identity.Roles.View, Identity.Roles.Create, Identity.Roles.Edit,
        
        // Role management
        RoleManagement.CreateRole, RoleManagement.EditRole, RoleManagement.AssignRole,
        RoleManagement.ManagePermissions
    };
    
    // Branch Manager permission set
    public static readonly HashSet<string> BranchManagerPermissions = new()
    {
        // Branch permissions
        Branches.View, Branches.Edit, Branches.ManageCustomers, Branches.ManageBranchUsers,
        Branches.ManageSettings, Branches.ViewReports,
        
        // Customer permissions
        Customers.View, Customers.Create, Customers.Edit, Customers.Delete,
        Customers.ManageAccounts, Customers.ViewHistory,
        
        // Limited identity permissions
        Identity.Users.View, Identity.Users.Create, Identity.Users.Edit,
        Identity.Users.ChangePassword, Identity.Users.ManageRoles,
        
        // Booking permissions
        Booking.View, Booking.Create, Booking.Edit, Booking.Delete,
        Booking.Cancel, Booking.Approve, Booking.Reject, Booking.ManagePayment,
        Booking.ViewHistory, Booking.ExportData,
        
        // Flight permissions
        Flight.View, Flight.ManageSeats
    };
    
    // Regular User permission set
    public static readonly HashSet<string> RegularUserPermissions = new()
    {
        // Customer permissions
        Customers.View, Customers.Create, Customers.Edit,
        
        // Booking permissions
        Booking.View, Booking.Create, Booking.Cancel, Booking.ViewHistory,
        
        // Flight permissions
        Flight.View
    };
    
    // Legacy mappings for backwards compatibility
    public static readonly HashSet<string> BrandLevelPermissions = BrandAdminPermissions;
    public static readonly HashSet<string> BranchLevelPermissions = BranchManagerPermissions;
} 