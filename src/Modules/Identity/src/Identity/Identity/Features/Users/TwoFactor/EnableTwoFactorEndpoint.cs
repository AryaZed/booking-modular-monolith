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
public class EnableTwoFactorEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public EnableTwoFactorEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("two-factor/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EnableTwoFactorResponse>> EnableTwoFactor(
        CancellationToken cancellationToken)
    {
        var command = new EnableTwoFactorCommand();
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
} 