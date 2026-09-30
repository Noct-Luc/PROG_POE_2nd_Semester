using RaceDay.Data;
using RaceDay.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Services
{
    public class AuthService : IAuthService
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User> RegisterAsync(string username, string email, string password, string role)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                throw new Exception("Username already exists");

            if (await _context.Users.AnyAsync(u => u.UserEmail == email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Username = username,
                UserEmail = email,
                UserRole = role,
                PasswordHash = HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            return user;
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }

        public bool VerifyPassword(string password, string hash)
        {
            var user = new User();
            var result = _passwordHasher.VerifyHashedPassword(user, hash, password);
            return result == PasswordVerificationResult.Success;
        }

        public string HashPassword(string password)
        {
            var user = new User();
            return _passwordHasher.HashPassword(user, password);
        }
    }
}