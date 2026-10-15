namespace RaceDay.Services;

public interface IAuthenticationService
{
    Task<RegisterResult> RegisterAsync(string username, string email, string password, string role);
    Task<LoginResult> LoginAsync(string username, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class RegisterResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Role { get; set; }
}