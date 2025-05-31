namespace Identity.Domain.Exceptions;

public class InvalidTenantOperationException : IdentityDomainException
{
    public InvalidTenantOperationException(string message) : base(message)
    {
    }

    public InvalidTenantOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 