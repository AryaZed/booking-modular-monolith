using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Authorization;

namespace Identity.Identity.Features.Users.AccountStatus;

[Route("api/identity/users")]
[ApiController]
[Authorize]
public class UnlockAccountEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public UnlockAccountEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("unlock-account/{userId}")]
    [RequirePermission("UserManagement.UnlockAccount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UnlockAccount(
        string userId,
        CancellationToken cancellationToken)
    {
        var command = new UnlockAccountCommand(userId);
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
}