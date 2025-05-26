using System;

namespace BuildingBlocks.Contracts.Identity;

/// <summary>
/// Integration event contract for when a user is deleted
/// </summary>
public class UserDeletedIntegrationEvent
{
    public long UserId { get; set; }
    public DateTime DeletedAt { get; set; }
} 