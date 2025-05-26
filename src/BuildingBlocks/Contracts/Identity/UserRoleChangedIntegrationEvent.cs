using System;

namespace BuildingBlocks.Contracts.Identity;

/// <summary>
/// Integration event contract for when a user's role is changed
/// </summary>
public class UserRoleChangedIntegrationEvent
{
    public long UserId { get; set; }
    public string Email { get; set; }
    public long RoleId { get; set; }
    public string RoleName { get; set; }
    public long TenantId { get; set; }
    public string TenantType { get; set; }
    public bool IsRoleAdded { get; set; }
    public DateTime ChangedAt { get; set; }
} 