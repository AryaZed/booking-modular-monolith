using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using BuildingBlocks.Domain;
using FluentValidation;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Users.AccountStatus;

public class GetAccountStatusQuery : IQuery<AccountStatusResponse>
{
    public long UserId { get; set; }
    
    public class Validator : AbstractValidator<GetAccountStatusQuery>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
        }
    }
}

public class GetAccountStatusQueryHandler : IQueryHandler<GetAccountStatusQuery, AccountStatusResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    
    public GetAccountStatusQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<AccountStatusResponse> Handle(GetAccountStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new NotFoundException("User not found");
            
        return new AccountStatusResponse
        {
            UserId = user.Id,
            Email = user.Email,
            IsActive = user.IsActive,
            IsLockedOut = await _userManager.IsLockedOutAsync(user),
            LockoutEnd = user.LockoutEnd,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
    }
}

public class AccountStatusResponse
{
    public long UserId { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
} 
