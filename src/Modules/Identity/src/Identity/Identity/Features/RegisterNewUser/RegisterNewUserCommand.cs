using System.Collections.Generic;
using Identity.Identity.Dtos;
using Identity.Identity.Models;
using MediatR;

namespace Identity.Identity.Features.RegisterNewUser;

public class RegisterNewUserCommand : IRequest<RegisterNewUserResponse>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string PassportNumber { get; set; }
    
    // Optional - for direct assignment to a tenant
    public long? TenantId { get; set; }
    public TenantType? TenantType { get; set; }
    public long? RoleId { get; set; }
}

public class RegisterNewUserResponse
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PassportNumber { get; set; }
    
    // If user was assigned to a tenant
    public IEnumerable<TenantAssignmentDto> TenantAssignments { get; set; } = new List<TenantAssignmentDto>();
}

public class TenantAssignmentDto
{
    public long TenantId { get; set; }
    public string TenantType { get; set; }
    public string RoleName { get; set; }
}
