
using System.Text.Json.Serialization;

namespace RaceDay.MVC.Models;

/// <summary>
/// Response from the API after successful login.
/// </summary>
public class LoginResponse
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("userRole")]
    public string UserRole { get; set; } = string.Empty;
}
