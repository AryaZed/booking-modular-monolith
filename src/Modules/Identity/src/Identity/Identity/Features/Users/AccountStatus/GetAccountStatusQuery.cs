using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Security;
using Identity.Identity.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.AccountStatus;

public record GetAccountStatusQuery() : IQuery<AccountStatusResponse>;

public record AccountStatusResponse(
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd,
    bool IsTwoFactorEnabled,
    bool IsEmailConfirmed,
    int AccessFailedCount,
    int MaxAllowedAccessFailedCount);

public class GetAccountStatusQueryHandler : IQueryHandler<GetAccountStatusQuery, AccountStatusResponse>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<GetAccountStatusQueryHandler> _logger;

    public GetAccountStatusQueryHandler(
        UserManager<Models.ApplicationUser> userManager,
        ICurrentUser currentUser,
        ILogger<GetAccountStatusQueryHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AccountStatusResponse> Handle(GetAccountStatusQuery request, CancellationToken cancellationToken)
    {
        // Get the current user
        var userId = _currentUser.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            _logger.LogWarning("Get account status attempted for non-existent user ID: {UserId}", userId);
            throw new IdentityException("User not found");
        }

        return new AccountStatusResponse(
            IsLockedOut: await _userManager.IsLockedOutAsync(user),
            LockoutEnd: user.LockoutEnd,
            IsTwoFactorEnabled: await _userManager.GetTwoFactorEnabledAsync(user),
            IsEmailConfirmed: await _userManager.IsEmailConfirmedAsync(user),
            AccessFailedCount: user.AccessFailedCount,
            MaxAllowedAccessFailedCount: _userManager.Options.Lockout.MaxFailedAccessAttempts
        );
    }
} 