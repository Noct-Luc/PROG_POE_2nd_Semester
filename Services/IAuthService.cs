using RaceDay.Models;

namespace RaceDay.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string username, string email, string password, string role);
        Task<User> LoginAsync(string username, string password);
        Task LogoutAsync();
        bool VerifyPassword(string password, string hash);
        string HashPassword(string password);
    }
}