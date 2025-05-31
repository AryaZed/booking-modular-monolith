using System.ComponentModel.DataAnnotations.Schema;
using BuildingBlocks.Domain.Model;
using Identity.Domain.Events;
using Identity.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Aggregates.User;

public class ApplicationUser : IdentityUser<long>, IAggregateRoot, IAuditableEntity, ISoftDeletableEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public string? PreferredLanguage { get; private set; }

    [NotMapped]
    private Email _email;
    
    [NotMapped]
    public Email Email
    {
        get => _email ?? Email.Create(base.Email);
        private set
        {
            _email = value;
            base.Email = value.Value;
            base.NormalizedEmail = value.Value.ToUpperInvariant();
        }
    }

    // Navigation properties
    public ICollection<UserTenantRole> TenantRoles { get; private set; } = new List<UserTenantRole>();

    private ApplicationUser()
    {
        // Required by EF Core
    }

    public static ApplicationUser Create(
        string userName, 
        Email email, 
        string firstName, 
        string lastName, 
        string? profileImageUrl = null)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ProfileImageUrl = profileImageUrl,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        user.AddDomainEvent(new UserCreatedEvent(0, userName, email, null));
        
        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string? profileImageUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        ProfileImageUrl = profileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(Email email)
    {
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLoginSuccess()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
    }

    public void RecordLoginFailure()
    {
        FailedLoginAttempts++;
        
        // Implement progressive lockout periods
        if (FailedLoginAttempts >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        }
    }

    public void SetPreferredLanguage(string languageCode)
    {
        PreferredLanguage = languageCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to get the default tenant role
    public UserTenantRole GetDefaultTenantRole()
    {
        return TenantRoles.FirstOrDefault(tr => tr.IsDefault) 
            ?? TenantRoles.FirstOrDefault();
    }

    // Method to add a new tenant role
    public void AddTenantRole(UserTenantRole userTenantRole, bool makeDefault = false)
    {
        if (makeDefault)
        {
            foreach (var existingRole in TenantRoles.Where(tr => tr.IsDefault))
            {
                existingRole.SetDefault(false);
            }
        }

        userTenantRole.SetDefault(makeDefault);
        TenantRoles.Add(userTenantRole);
        UpdatedAt = DateTime.UtcNow;
    }
} 