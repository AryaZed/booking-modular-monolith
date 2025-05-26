using System;
using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

public enum UserStatusChangeType
{
    Activated,
    Deactivated,
    Deleted
}

public class UserStatusChangedEvent : IDomainEvent
{
    public long UserId { get; }
    public UserStatusChangeType ChangeType { get; }
    public DateTime ChangedAt { get; }
    public long ChangedBy { get; }
    
    public UserStatusChangedEvent(
        long userId,
        UserStatusChangeType changeType,
        long changedBy)
    {
        UserId = userId;
        ChangeType = changeType;
        ChangedAt = DateTime.UtcNow;
        ChangedBy = changedBy;
    }
} 