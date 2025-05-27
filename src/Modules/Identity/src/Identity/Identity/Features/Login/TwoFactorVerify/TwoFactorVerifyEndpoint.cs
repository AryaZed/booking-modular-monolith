using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Login.TwoFactorVerify;

[Route("api/identity")]
[ApiController]
public class TwoFactorVerifyEndpoint : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TwoFactorVerifyEndpoint> _logger;

    public TwoFactorVerifyEndpoint(IMediator mediator, ILogger<TwoFactorVerifyEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login/2fa/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TwoFactorVerifyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TwoFactorVerifyResponse>> VerifyTwoFactor(
        [FromBody] TwoFactorVerifyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new TwoFactorVerifyCommand
            {
                UserId = request.UserId,
                OtpCode = request.OtpCode,
                TempToken = request.TempToken,
                TenantId = request.TenantId,
                TenantType = request.TenantType
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
            _logger.LogError(ex, "Error verifying two-factor authentication");
            return BadRequest(new TwoFactorVerifyResponse
            {
                Success = false,
                Message = $"Error verifying two-factor authentication: {ex.Message}"
            });
        }
    }
}

public class TwoFactorVerifyRequest
{
    public string UserId { get; set; }
    public string OtpCode { get; set; }
    public string TempToken { get; set; }
    public long? TenantId { get; set; }
    public string? TenantType { get; set; }
} 