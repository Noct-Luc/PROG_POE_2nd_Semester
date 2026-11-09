namespace RaceDay.MVC.Models;

/// <summary>
/// Generic error response from the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}