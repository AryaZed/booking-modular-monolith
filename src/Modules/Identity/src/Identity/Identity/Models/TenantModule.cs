using System;

namespace Identity.Identity.Models;

public class TenantModule
{
    private TenantModule() { }

    public long Id { get; private set; }
    public long TenantId { get; private set; }
    public long ModuleId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime SubscribedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public virtual Tenant Tenant { get; private set; }
    public virtual Module Module { get; private set; }

    public static TenantModule Create(
        long tenantId,
        long moduleId,
        DateTime? expiresAt = null)
    {
        return new TenantModule
        {
            TenantId = tenantId,
            ModuleId = moduleId,
            IsActive = true,
            SubscribedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Renew(DateTime? newExpiryDate)
    {
        if (DeactivatedAt.HasValue)
        {
            // Reactivate subscription
            IsActive = true;
            DeactivatedAt = null;
        }
        
        ExpiresAt = newExpiryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            DeactivatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            DeactivatedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    public bool HasAccess()
    {
        return IsActive && (!ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow);
    }
} 