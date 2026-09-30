using RaceDay.Models;

namespace RaceDay.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task<User> UpdateUserAsync(int userId, string username, string email, string role);
        Task<bool> DeleteUserAsync(int userId);
    }
}