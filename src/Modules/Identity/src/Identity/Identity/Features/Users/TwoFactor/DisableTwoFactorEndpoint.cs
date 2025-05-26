using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Users.TwoFactor;

[Route("api/identity/users")]
[ApiController]
[Authorize]
public class DisableTwoFactorEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public DisableTwoFactorEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("two-factor/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DisableTwoFactor(
        [FromBody] DisableTwoFactorCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
} 