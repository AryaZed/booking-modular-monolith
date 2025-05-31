using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Model;

namespace Identity.Domain.ValueObjects;

public class Password : ValueObject
{
    // Hash of the password is stored, never the plain text
    public string PasswordHash { get; private set; }

    private Password(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    // Factory method to create from a hash (for loading from DB)
    public static Password FromHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash), "Password hash cannot be empty");

        return new Password(passwordHash);
    }
    
    // For creating a new password, the validation is done here, but the actual hashing
    // will be done by the infrastructure layer
    public static bool IsValidPassword(string password, out List<string> validationErrors)
    {
        validationErrors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(password))
        {
            validationErrors.Add("Password cannot be empty");
            return false;
        }

        if (password.Length < 12)
            validationErrors.Add("Password must be at least 12 characters long");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            validationErrors.Add("Password must contain at least one uppercase letter");

        if (!Regex.IsMatch(password, @"[a-z]"))
            validationErrors.Add("Password must contain at least one lowercase letter");

        if (!Regex.IsMatch(password, @"[0-9]"))
            validationErrors.Add("Password must contain at least one digit");

        if (!Regex.IsMatch(password, @"[^A-Za-z0-9]"))
            validationErrors.Add("Password must contain at least one special character");

        return validationErrors.Count == 0;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PasswordHash;
    }
} 