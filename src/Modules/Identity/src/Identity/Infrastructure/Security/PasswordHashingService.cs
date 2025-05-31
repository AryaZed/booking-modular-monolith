using Identity.Domain.Services;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Security;

public class PasswordHashingService : IPasswordHashingService
{
    private readonly IPasswordHasher<object> _passwordHasher;

    public PasswordHashingService(IPasswordHasher<object> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success || 
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
} 