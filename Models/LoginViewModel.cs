using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

/// <summary>
/// View model for user login authentication.
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Gets or sets the username for login.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for login.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to remember the user's login.
    /// </summary>
    public bool RememberMe { get; set; }
}