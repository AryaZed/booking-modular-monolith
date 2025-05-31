using BuildingBlocks.Authorization;
using BuildingBlocks.Exception;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Identity.Features.Login;

[Route("api/identity")]
public class LoginEndpoint : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;

    public LoginEndpoint(IMediator mediator, ITokenService tokenService)
    {
        _mediator = mediator;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (IdentityException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (IdentityException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
} 