using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Users.ResetPassword;

[Route("api/identity/users")]
[ApiController]
public class ResetPasswordEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public ResetPasswordEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
} 