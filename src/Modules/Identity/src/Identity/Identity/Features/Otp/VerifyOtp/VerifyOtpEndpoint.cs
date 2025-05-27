using Identity.Identity.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Otp.VerifyOtp;

[Route("api/identity/otp")]
[ApiController]
public class VerifyOtpEndpoint : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VerifyOtpEndpoint> _logger;

    public VerifyOtpEndpoint(IMediator mediator, ILogger<VerifyOtpEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("verify")]
    [AllowAnonymous] // Allow anonymous access for OTP verification
    [ProducesResponseType(typeof(VerifyOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new VerifyOtpCommand
            {
                UserId = request.UserId,
                Code = request.Code,
                Purpose = request.Purpose
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
            _logger.LogError(ex, "Error verifying OTP");
            return BadRequest(new VerifyOtpResponse
            {
                Success = false,
                Message = $"Error verifying OTP: {ex.Message}"
            });
        }
    }
}

public class VerifyOtpRequest
{
    public string UserId { get; set; }
    public string Code { get; set; }
    public OtpPurpose Purpose { get; set; }
} 