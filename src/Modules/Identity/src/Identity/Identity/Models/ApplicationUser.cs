using Microsoft.AspNetCore.Identity;
using BuildingBlocks.Domain.Model;
using Identity.Identity.Events;
using BuildingBlocks.Domain.Event;
using BuildingBlocks.Domain;

namespace Identity.Identity.Models;

public class ApplicationUser : IdentityUser<long>, IAuditableEntity, ISoftDeletableEntity, IEntityWithDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PassPortNumber { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public long? DeletedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public long? LastModifiedBy { get; private set; }

    // Navigation property for tenant associations
    public virtual ICollection<UserTenantRole> TenantRoles { get; private set; } = new List<UserTenantRole>();

    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Domain methods with validation
    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainValidationException("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainValidationException("Last name cannot be empty");

        FirstName = firstName;
        LastName = lastName;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetPassportNumber(string passportNumber)
    {
        // Add validation logic here
        PassPortNumber = passportNumber;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate(long modifiedBy)
    {
        if (!IsActive)
            return;

        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        _domainEvents.Add(new UserStatusChangedEvent(Id, UserStatusChangeType.Deactivated, modifiedBy));
    }

    public void Activate(long modifiedBy)
    {
        if (IsActive)
            return;

        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        _domainEvents.Add(new UserStatusChangedEvent(Id, UserStatusChangeType.Activated, modifiedBy));
    }

    public void SoftDelete(long deletedBy)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;

        _domainEvents.Add(new UserStatusChangedEvent(Id, UserStatusChangeType.Deleted, deletedBy));
    }

    // Factory method for creating users
    public static ApplicationUser Create(string email, string firstName, string lastName, long? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email cannot be empty");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainValidationException("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainValidationException("Last name cannot be empty");

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        user._domainEvents.Add(new UserRegisteredEvent(0, email, email));

        return user;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
