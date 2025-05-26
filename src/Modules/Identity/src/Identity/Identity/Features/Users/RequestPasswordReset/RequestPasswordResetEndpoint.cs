using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Users.RequestPasswordReset;

[Route("api/identity/users")]
[ApiController]
public class RequestPasswordResetEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public RequestPasswordResetEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
} 