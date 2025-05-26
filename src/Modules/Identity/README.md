# Identity Module

This module provides identity and authorization services for the application, supporting a multi-tenant architecture with a flexible permission-based authorization system.

## Architecture

The Identity module is built using the following architecture:

### Domain Models

- **ApplicationUser**: Extends IdentityUser with additional properties and relationships
- **ApplicationRole**: Extends IdentityRole with tenant-specific properties and permissions
- **UserTenantRole**: Associates users with tenants and roles, enabling users to have different roles across tenants
- **RolePermission**: Associates roles with specific permissions

### Core Identity Services

- **IdentityServer**: Provides OAuth2/OpenID Connect authentication
- **Permission-based Authorization**: Custom authorization system based on permissions
- **Multi-tenant Access Control**: Manages user access across different tenants

## Key Components

### Token Authentication

JWT-based token authentication with:

- Standard identity claims (user ID, name, email)
- Tenant context claims (tenant ID, tenant type)
- Role claims
- Permission claims

### Permission-based Authorization

Rather than relying solely on roles, the system uses granular permissions:

- Each role has a set of assigned permissions
- API endpoints are protected by permission requirements
- Permission requirements are enforced using custom authorization handlers
- Permissions can be checked both declaratively and imperatively

### Multi-tenant Support

The module supports users having different roles across multiple tenants:

- Users can be assigned different roles in different tenants
- Users can switch between tenant contexts
- Authorization is tenant-aware

## API Endpoints

### Authentication

- `POST /api/identity/token`: Authenticate and get a token
- `POST /api/identity/switch-tenant`: Switch to a different tenant context

### Roles and Permissions

- `GET /api/identity/permissions`: List all available permissions
- `GET /api/identity/roles/{roleId}/permissions`: Get permissions for a role
- `POST /api/identity/tenants/{tenantId}/roles`: Create a new role for a tenant

### User Management

- `POST /api/identity/tenants/{tenantId}/users/{userId}/roles`: Assign a user to a tenant with a role

## Best Practices

### 1. Separation of Concerns

- Identity concerns (authentication, authorization) are separated from business domain concerns
- Business domain models (like Brand, Branch) should be moved to their own modules

### 2. Clean Architecture

- Features are organized by domain concept and use case
- Each feature has its own folder with endpoint, command/query, and handler

### 3. Permission-based Authorization

- Use permissions instead of roles for authorization
- Define permissions as constants in a central location
- Use the `[RequirePermission]` attribute for declarative authorization
- Use the `User.HasPermission()` extension method for imperative checks

### 4. Claims-based Identity

- Store identity information in claims
- Use extension methods to access claims (e.g., `User.GetUserId()`)
- Include tenant context in claims

### 5. Dependency Injection

- Use interfaces for services to enable mocking and testing
- Register services with appropriate lifecycles

## Example Usage

### Protecting an Endpoint with Permissions

```csharp
[HttpPost]
[Authorize]
[RequirePermission(PermissionsConstant.RoleManagement.CreateRole)]
public async Task<ActionResult> CreateRole(CreateRoleCommand command)
{
    // Implementation
}
```

### Checking Permissions in Code

```csharp
if (User.HasPermission(PermissionsConstant.RoleManagement.EditRole))
{
    // Allow editing
}
```

### Accessing User Information

```csharp
var userId = User.GetUserId();
var email = User.GetEmail();
var tenantId = User.GetTenantId();
``` 