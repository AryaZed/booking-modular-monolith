namespace Identity.Identity.Models;

public sealed class Module
{
    private Module() { }

    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Module Create(string name, string code, string description)
    {
        return new Module
        {
            Name = name,
            Code = code.ToLower(System.Globalization.CultureInfo.CurrentCulture),
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
} 
