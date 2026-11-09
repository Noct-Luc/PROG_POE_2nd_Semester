namespace RaceDay.MVC.Models;

/// <summary>
/// Data transfer object for user profile.
/// </summary>
public class UserProfileDto
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}