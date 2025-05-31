using BuildingBlocks.Domain.Model;

namespace Identity.Domain.Aggregates.Tenant;

public class Module : Entity
{
    public string Name { get; private set; }
    public string Key { get; private set; }
    public bool IsEnabled { get; private set; }
    public string Description { get; private set; }

    // Navigation properties
    public ICollection<TenantModule> Tenants { get; private set; } = new List<TenantModule>();

    private Module()
    {
        // Required by EF Core
    }

    public static Module Create(string name, string key, string description = null, bool isEnabled = true)
    {
        return new Module
        {
            Name = name,
            Key = key.ToLowerInvariant(),
            Description = description,
            IsEnabled = isEnabled
        };
    }

    public void Update(string name, string description, bool isEnabled)
    {
        Name = name;
        Description = description;
        IsEnabled = isEnabled;
    }
} 