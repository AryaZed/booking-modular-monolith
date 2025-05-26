using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Web;
using Identity.Identity.Dtos;
using Identity.Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Token;

[Route(BaseApiPath + "/identity/token")]
public class TokenEndpoint : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    
    public TokenEndpoint(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(
        Summary = "Authenticate user",
        Description = "Authenticates a user with username/password and returns a JWT token with refresh token",
        Tags = new[] { "Authentication" })]
    public async Task<ActionResult<TokenResponse>> GetToken(
        [FromBody] TokenRequest request,
        CancellationToken cancellationToken)
    {
        // Validate the request
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "Username and password are required" });
            
        // Find the user
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
            return Unauthorized(new { error = "Invalid username or password" });
            
        // Verify the password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Unauthorized(new { error = "Invalid username or password" });
            
        // Ensure the user is active
        if (!user.IsActive)
            return Unauthorized(new { error = "Account is disabled" });
            
        // Generate the token
        var response = await _tokenService.GenerateTokenAsync(user);
        
        return Ok(response);
    }
} 