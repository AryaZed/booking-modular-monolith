using System.Text.Json.Serialization;

namespace Identity.Identity.Features.RefreshToken;

public class RevokeTokenRequest
{
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
} 