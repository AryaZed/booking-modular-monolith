using System;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

/// <summary>
/// Event published when a user is deleted
/// </summary>
public class UserDeletedEvent : IDomainEvent
{
    public long UserId { get; }
    public long DeletedBy { get; }
    public DateTime DeletedAt { get; }
    
    public UserDeletedEvent(long userId, long deletedBy)
    {
        UserId = userId;
        DeletedBy = deletedBy;
        DeletedAt = DateTime.UtcNow;
    }
} 