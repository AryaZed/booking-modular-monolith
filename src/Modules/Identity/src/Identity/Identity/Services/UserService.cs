using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using FluentValidation;
using Identity.Identity.Events;
using Identity.Identity.Models;
using Identity.Identity.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Identity.Identity.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IEventDispatcher _eventDispatcher;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEventDispatcher eventDispatcher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        public async Task<ApplicationUser> RegisterNewUserAsync(
            string email,
            string firstName,
            string lastName,
            string password,
            string username = null,
            string passportNumber = null,
            CancellationToken cancellationToken = default)
        {
            // Check if user already exists
            if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            {
                throw new ValidationException("A user with this email already exists");
            }

            // Create new user
            var user = ApplicationUser.Create(
                email: email,
                firstName: firstName,
                lastName: lastName
            );

            // Set additional properties
            user.UserName = !string.IsNullOrEmpty(username) ? username : email;

            if (!string.IsNullOrEmpty(passportNumber))
            {
                user.SetPassportNumber(passportNumber);
            }

            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            // Add user to database
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // Publish UserCreatedEvent
            await _eventDispatcher.DispatchAsync(new UserCreatedEvent(
                userId: user.Id,
                email: user.Email,
                username: user.UserName
            ));

            return user;
        }

        public async Task AssignUserToTenantWithRoleAsync(
            long userId,
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new ValidationException($"User with id {userId} not found");
            }

            var userTenantRole = UserTenantRole.Create(
                userId: userId,
                tenantId: tenantId,
                roleId: roleId,
                createdBy: 0 // System user
            );

            user.AddTenantRole(userTenantRole);
            
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }
} 