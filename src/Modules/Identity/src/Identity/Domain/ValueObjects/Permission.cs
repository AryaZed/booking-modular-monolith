using BuildingBlocks.Domain.Model;

namespace Identity.Domain.ValueObjects;

public class Permission : ValueObject
{
    public string Name { get; private set; }
    public string GroupName { get; private set; }
    public string Description { get; private set; }

    private Permission(string name, string groupName, string description)
    {
        Name = name;
        GroupName = groupName;
        Description = description;
    }

    public static Permission Create(string name, string groupName, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "Permission name cannot be empty");

        if (string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentNullException(nameof(groupName), "Permission group name cannot be empty");

        description ??= string.Empty;

        return new Permission(name, groupName, description);
    }

    public static Permission FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "Permission name cannot be empty");
            
        // Extract group name from permission name format "GroupName.PermissionName"
        string groupName = "System";
        string description = string.Empty;
        
        string[] parts = name.Split('.');
        if (parts.Length > 1)
        {
            groupName = parts[0];
        }

        return new Permission(name, groupName, description);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }

    public static implicit operator string(Permission permission) => permission.Name;
    
    public override string ToString() => Name;
} 