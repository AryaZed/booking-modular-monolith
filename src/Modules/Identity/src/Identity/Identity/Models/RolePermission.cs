using System;
using BuildingBlocks.Domain.Model;

namespace Identity.Identity.Models;

public class RolePermission : IAuditableEntity
{
    public long Id { get; private set; }
    public long RoleId { get; private set; }
    public string Permission { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public long? LastModifiedBy { get; private set; }
    
    // Navigation property
    public virtual ApplicationRole Role { get; private set; }
    
    // Factory method
    public static RolePermission Create(long roleId, string permission, long? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new DomainValidationException("Permission cannot be empty");
            
        return new RolePermission
        {
            RoleId = roleId,
            Permission = permission,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
} 