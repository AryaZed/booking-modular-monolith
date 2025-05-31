using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Model;

namespace Identity.Domain.ValueObjects;

public class TenantKey : ValueObject
{
    public string Value { get; private set; }

    private TenantKey(string value)
    {
        Value = value;
    }

    public static TenantKey Create(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key), "Tenant key cannot be empty");

        key = key.Trim().ToLowerInvariant();

        if (key.Length < 3)
            throw new ArgumentException("Tenant key must be at least 3 characters", nameof(key));

        if (key.Length > 50)
            throw new ArgumentException("Tenant key cannot be longer than 50 characters", nameof(key));

        if (!IsValidKey(key))
            throw new ArgumentException("Tenant key can only contain lowercase letters, numbers, and hyphens", nameof(key));

        return new TenantKey(key);
    }

    private static bool IsValidKey(string key)
    {
        // Allow only lowercase letters, numbers, and hyphens
        string pattern = @"^[a-z0-9\-]+$";
        return Regex.IsMatch(key, pattern);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(TenantKey tenantKey) => tenantKey.Value;
    
    public override string ToString() => Value;
} 