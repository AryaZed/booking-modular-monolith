using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Web;
using Identity.Identity.Dtos;
using Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.RefreshToken;

[Route(BaseApiPath + "/identity/token")]
public class RefreshTokenEndpoint : BaseController
{
    private readonly ITokenService _tokenService;
    
    public RefreshTokenEndpoint(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Refresh token",
        Description = "Generates a new access token using a refresh token",
        Tags = new[] { "Authentication" })]
    public async Task<ActionResult<TokenResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _tokenService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Revoke token",
        Description = "Revokes a refresh token",
        Tags = new[] { "Authentication" })]
    public async Task<ActionResult> RevokeToken(
        [FromBody] RevokeTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _tokenService.RevokeTokenAsync(request.RefreshToken);
        if (result)
            return Ok(new { message = "Token revoked" });
            
        return BadRequest(new { error = "Token not found or already revoked" });
    }
} 