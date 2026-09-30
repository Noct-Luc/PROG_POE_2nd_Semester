using RaceDay.Data;
using RaceDay.Models;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Services
{
    public class UserService : IUserService
    {
        private readonly RaceDayDbContext _context;

        public UserService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async Task<User> UpdateUserAsync(int userId, string username, string email, string role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
                return null;

            user.Username = username;
            user.UserEmail = email;
            user.UserRole = role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}