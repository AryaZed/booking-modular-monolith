using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Users.AccountStatus;

[Route("api/identity/users")]
[ApiController]
[Authorize]
public class GetAccountStatusEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetAccountStatusEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("account-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountStatusResponse>> GetAccountStatus(
        CancellationToken cancellationToken)
    {
        var query = new GetAccountStatusQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
} 