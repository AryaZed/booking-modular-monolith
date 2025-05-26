using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using BuildingBlocks.Domain.Model;
using BuildingBlocks.Domain;
using Identity.Identity.Events;

namespace Identity.Identity.Models;

public class ApplicationRole : IdentityRole<long>, IAuditableEntity, ISoftDeletableEntity, IEntityWithDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public long? TenantId { get; private set; }  // Brand/Partner ID
    public bool IsCustom { get; set; }   // Whether this is a custom role created by a B2B partner
    public string Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    // Audit properties
    public DateTime CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public long? LastModifiedBy { get; private set; }
    
    // Soft delete properties
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public long? DeletedBy { get; private set; }
    
    // Navigation properties
    public virtual ICollection<RolePermission> Permissions { get; private set; } = new List<RolePermission>();
    public virtual ICollection<UserTenantRole> UserTenantRoles { get; private set; } = new List<UserTenantRole>();
    
    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    // Domain methods
    public void UpdateDescription(string description, long modifiedBy)
    {
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
    
    public void Activate(long modifiedBy)
    {
        if (IsActive)
            return;
            
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
    
    public void Deactivate(long modifiedBy)
    {
        if (!IsActive)
            return;
            
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
    
    public void SoftDelete(long deletedBy)
    {
        if (IsDeleted)
            return;
            
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
    
    public void AddPermission(string permission, long modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new DomainValidationException("Permission cannot be empty");
            
        // Check if permission already exists for this role
        foreach (var existingPermission in Permissions)
        {
            if (existingPermission.Permission == permission)
                return; // Permission already exists, no need to add it
        }
        
        var rolePermission = RolePermission.Create(Id, permission, modifiedBy);
        Permissions.Add(rolePermission);
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        _domainEvents.Add(new PermissionUpdatedEvent(
            Id, 
            Name, 
            TenantId, 
            PermissionUpdateType.Added, 
            new[] { permission }, 
            modifiedBy));
    }
    
    public void AddPermissions(IEnumerable<string> permissions, long modifiedBy)
    {
        if (permissions == null)
            throw new ArgumentNullException(nameof(permissions));
            
        var addedPermissions = new List<string>();
        
        foreach (var permission in permissions)
        {
            if (string.IsNullOrWhiteSpace(permission))
                continue;
                
            // Check if permission already exists for this role
            bool exists = false;
            foreach (var existingPermission in Permissions)
            {
                if (existingPermission.Permission == permission)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                var rolePermission = RolePermission.Create(Id, permission, modifiedBy);
                Permissions.Add(rolePermission);
                addedPermissions.Add(permission);
            }
        }
        
        if (addedPermissions.Count > 0)
        {
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            
            _domainEvents.Add(new PermissionUpdatedEvent(
                Id, 
                Name, 
                TenantId, 
                PermissionUpdateType.Added, 
                addedPermissions, 
                modifiedBy));
        }
    }
    
    public void RemovePermission(string permission, long modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new DomainValidationException("Permission cannot be empty");
            
        // Find permission to remove (must be done outside of the Permissions collection)
        RolePermission permissionToRemove = null;
        
        foreach (var existingPermission in Permissions)
        {
            if (existingPermission.Permission == permission)
            {
                permissionToRemove = existingPermission;
                break;
            }
        }
        
        if (permissionToRemove != null)
        {
            Permissions.Remove(permissionToRemove);
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            
            _domainEvents.Add(new PermissionUpdatedEvent(
                Id, 
                Name, 
                TenantId, 
                PermissionUpdateType.Removed, 
                new[] { permission }, 
                modifiedBy));
        }
    }
    
    public void RemovePermissions(IEnumerable<string> permissions, long modifiedBy)
    {
        if (permissions == null)
            throw new ArgumentNullException(nameof(permissions));
            
        var removedPermissions = new List<string>();
        
        foreach (var permission in permissions)
        {
            if (string.IsNullOrWhiteSpace(permission))
                continue;
                
            // Find permission to remove
            RolePermission permissionToRemove = null;
            
            foreach (var existingPermission in Permissions)
            {
                if (existingPermission.Permission == permission)
                {
                    permissionToRemove = existingPermission;
                    break;
                }
            }
            
            if (permissionToRemove != null)
            {
                Permissions.Remove(permissionToRemove);
                removedPermissions.Add(permission);
            }
        }
        
        if (removedPermissions.Count > 0)
        {
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            
            _domainEvents.Add(new PermissionUpdatedEvent(
                Id, 
                Name, 
                TenantId, 
                PermissionUpdateType.Removed, 
                removedPermissions, 
                modifiedBy));
        }
    }
    
    // Factory method
    public static ApplicationRole Create(string name, string description, bool isDefault, long? tenantId = null, long? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Role name cannot be empty");
            
        return new ApplicationRole
        {
            Name = name,
            NormalizedName = name.ToUpper(),
            Description = description,
            IsDefault = isDefault,
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 