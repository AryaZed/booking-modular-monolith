using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Identity.Identity.Features.Token;

public class TokenRequest
{
    [Required]
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; }
} 