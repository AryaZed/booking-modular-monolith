using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Login.TwoFactorLogin;

[Route("api/identity")]
[ApiController]
public class TwoFactorLoginEndpoint : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TwoFactorLoginEndpoint> _logger;

    public TwoFactorLoginEndpoint(IMediator mediator, ILogger<TwoFactorLoginEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login/2fa/init")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TwoFactorLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TwoFactorLoginResponse>> InitiateTwoFactorLogin(
        [FromBody] TwoFactorLoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new TwoFactorLoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating two-factor login");
            return BadRequest(new TwoFactorLoginResponse
            {
                Success = false,
                Message = $"Error initiating two-factor login: {ex.Message}"
            });
        }
    }
}

public class TwoFactorLoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
} 