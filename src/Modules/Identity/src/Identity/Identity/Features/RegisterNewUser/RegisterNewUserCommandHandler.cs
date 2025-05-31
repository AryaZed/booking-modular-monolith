using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Identity.Identity.Dtos;
using Identity.Identity.Repositories;
using Identity.Identity.Services;
using MediatR;

namespace Identity.Identity.Features.RegisterNewUser;

public class RegisterNewUserCommandHandler : IRequestHandler<RegisterNewUserCommand, RegisterNewUserResponse>
{
    private readonly UserService _userService;
    private readonly IRoleRepository _roleRepository;

    public RegisterNewUserCommandHandler(
        UserService userService,
        IRoleRepository roleRepository)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    public async Task<RegisterNewUserResponse> Handle(RegisterNewUserCommand command, CancellationToken cancellationToken)
    {
        // Register the new user using the service
        var user = await _userService.RegisterNewUserAsync(
            email: command.Email,
            firstName: command.FirstName,
            lastName: command.LastName,
            password: command.Password,
            username: command.Username,
            passportNumber: command.PassportNumber,
            cancellationToken: cancellationToken
        );

        var tenantAssignments = new List<TenantAssignmentDto>();

        // If tenant information is provided, assign user to tenant with specified role
        if (command.TenantId.HasValue && command.TenantType.HasValue && command.RoleId.HasValue)
        {
            var role = await _roleRepository.GetByIdAsync(command.RoleId.Value, cancellationToken);
            if (role != null)
            {
                await _userService.AssignUserToTenantWithRoleAsync(
                    userId: user.Id,
                    tenantId: command.TenantId.Value,
                    roleId: role.Id,
                    cancellationToken: cancellationToken
                );

                // Add to response
                tenantAssignments.Add(new TenantAssignmentDto
                {
                    TenantId = command.TenantId.Value,
                    TenantType = command.TenantType.Value.ToString(),
                    RoleName = role.Name
                });
            }
        }

        return new RegisterNewUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.UserName,
            PassportNumber = user.PassPortNumber,
            TenantAssignments = tenantAssignments
        };
    }
}
