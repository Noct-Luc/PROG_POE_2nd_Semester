using RaceDay.Data;
using RaceDay.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace RaceDay.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly RaceDayDbContext _context;

    public AuthenticationService(RaceDayDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterResult> RegisterAsync(string username, string email, string password, string role)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.UserEmail == email);

            if (existingUser != null)
                return new RegisterResult
                {
                    Success = false,
                    Message = "Username or email already exists"
                };

            // Create new user
            var hashedPassword = HashPassword(password);
            var user = new User
            {
                Username = username,
                UserEmail = email,
                PasswordHash = hashedPassword,
                UserRole = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterResult
            {
                Success = true,
                Message = "User registered successfully",
                UserId = user.UserID
            };
        }
        catch (Exception ex)
        {
            return new RegisterResult
            {
                Success = false,
                Message = $"Registration failed: {ex.Message}"
            };
        }
    }

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };

            if (!VerifyPassword(password, user.PasswordHash))
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };

            return new LoginResult
            {
                Success = true,
                Message = "Login successful",
                UserId = user.UserID,
                Role = user.UserRole
            };
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                Success = false,
                Message = $"Login failed: {ex.Message}"
            };
        }
    }

    public string HashPassword(string password)
    {
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashWithSalt = new byte[36];
            Array.Copy(salt, 0, hashWithSalt, 0, 16);
            Array.Copy(hash, 0, hashWithSalt, 16, 20);

            return Convert.ToBase64String(hashWithSalt);
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        byte[] hashWithSalt = Convert.FromBase64String(hash);
        byte[] salt = new byte[16];
        Array.Copy(hashWithSalt, 0, salt, 0, 16);

        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
        {
            byte[] hash2 = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashWithSalt[i + 16] != hash2[i])
                    return false;
            }

            return true;
        }
    }
}