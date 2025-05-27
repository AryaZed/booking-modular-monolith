using System;

namespace BuildingBlocks.Contracts.Identity;

/// <summary>
/// Integration event contract for when a user is created
/// This is a contract that both Identity (publisher) and other modules (subscribers) can reference
/// without creating direct dependencies between modules
/// </summary>
public class UserCreatedIntegrationEvent
{
    public long UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public long? TenantId { get; set; }
    public string TenantType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
} 
