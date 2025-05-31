using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Model;

namespace Identity.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email cannot be empty");

        email = email.Trim();

        if (email.Length > 256)
            throw new ArgumentException("Email cannot be longer than 256 characters", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Email is not in a valid format", nameof(email));

        return new Email(email);
    }

    private static bool IsValidEmail(string email)
    {
        // Simple regex for email validation
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
    
    public override string ToString() => Value;
} 