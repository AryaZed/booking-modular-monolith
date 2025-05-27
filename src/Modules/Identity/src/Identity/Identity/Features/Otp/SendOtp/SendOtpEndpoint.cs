using Identity.Identity.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Otp.SendOtp;

[Route("api/identity/otp")]
[ApiController]
public class SendOtpEndpoint : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SendOtpEndpoint> _logger;

    public SendOtpEndpoint(IMediator mediator, ILogger<SendOtpEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("send")]
    [Authorize(Roles = "admin,system_admin")]
    [ProducesResponseType(typeof(SendOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SendOtpResponse>> SendOtp(
        [FromBody] SendOtpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new SendOtpCommand
            {
                UserId = request.UserId,
                Purpose = request.Purpose,
                Reference = request.Reference
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
            _logger.LogError(ex, "Error sending OTP");
            return BadRequest(new SendOtpResponse
            {
                Success = false,
                Message = $"Error sending OTP: {ex.Message}"
            });
        }
    }
}

public class SendOtpRequest
{
    public string UserId { get; set; }
    public OtpPurpose Purpose { get; set; }
    public string? Reference { get; set; }
} 