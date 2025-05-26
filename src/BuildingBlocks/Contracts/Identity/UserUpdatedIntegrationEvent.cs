using System;

namespace BuildingBlocks.Contracts.Identity;

/// <summary>
/// Integration event contract for when a user is updated
/// </summary>
public class UserUpdatedIntegrationEvent
{
    public long UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
} 