using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Controllers;
using RaceDay.Data;
using RaceDay.Models;
using RaceDay.Services;
using Xunit;
using Moq;

namespace RaceDay.Tests;

/// <summary>
/// Unit tests for RaceDay API controllers and services.
/// </summary>
public class ApiUnitTests
{
    /// <summary>
    /// Helper method to create a clean In-Memory database context for each test.
    /// </summary>
    private RaceDayDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var databaseContext = new RaceDayDbContext(options);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    #region Authentication Service Tests

    [Fact]
    public async Task RegisterAsync_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        
        const string username = "testuser";
        const string email = "test@example.com";
        const string password = "SecurePassword123!";
        const string role = "Participant";

        // Act
        var result = await authService.RegisterAsync(username, email, password, role);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.UserId);
        Assert.Equal("User registered successfully", result.Message);
        
        // Verify user was added to database
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(user);
        Assert.Equal(email, user.UserEmail);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFail()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        
        const string username = "existinguser";
        const string email1 = "test1@example.com";
        const string email2 = "test2@example.com";
        const string password = "SecurePassword123!";

        // Register first user
        await authService.RegisterAsync(username, email1, password, "Participant");

        // Act - Try to register with same username
        var result = await authService.RegisterAsync(username, email2, password, "Participant");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Username or email already exists", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFail()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        
        const string username1 = "user1";
        const string username2 = "user2";
        const string email = "duplicate@example.com";
        const string password = "SecurePassword123!";

        // Register first user
        await authService.RegisterAsync(username1, email, password, "Participant");

        // Act - Try to register with same email
        var result = await authService.RegisterAsync(username2, email, password, "Participant");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Username or email already exists", result.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        
        const string username = "testuser";
        const string email = "test@example.com";
        const string password = "SecurePassword123!";
        const string role = "Manager";

        // Register user first
        await authService.RegisterAsync(username, email, password, role);

        // Act
        var result = await authService.LoginAsync(username, password);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.UserId);
        Assert.Equal(role, result.Role);
        Assert.Equal("Login successful", result.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ReturnsFail()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);

        // Act
        var result = await authService.LoginAsync("nonexistent", "password");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFail()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        
        const string username = "testuser";
        const string email = "test@example.com";
        const string password = "SecurePassword123!";

        // Register user first
        await authService.RegisterAsync(username, email, password, "Participant");

        // Act
        var result = await authService.LoginAsync(username, "WrongPassword");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
    }

    [Fact]
    public void HashPassword_ProducesConsistentLength()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        const string password = "TestPassword123!";

        // Act
        var hash1 = authService.HashPassword(password);
        var hash2 = authService.HashPassword(password);

        // Assert - Hashes should be different due to random salt
        Assert.NotEqual(hash1, hash2);
        // But same length (Base64 encoded 36 bytes)
        Assert.Equal(hash1.Length, hash2.Length);
    }

    [Fact]
    public void VerifyPassword_ValidPassword_ReturnsTrue()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        const string password = "TestPassword123!";

        var hash = authService.HashPassword(password);

        // Act
        var result = authService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var db = GetDatabaseContext();
        var authService = new AuthenticationService(db);
        const string password = "TestPassword123!";

        var hash = authService.HashPassword(password);

        // Act
        var result = authService.VerifyPassword("WrongPassword", hash);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Auth Controller Tests

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var db = GetDatabaseContext();
        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new RegisterResult { Success = true, UserId = 1, Message = "User registered successfully" });

        var controller = new AuthController(mockAuthService.Object);

        var request = new RegisterRequest
        {
            Username = "newuser",
            UserEmail = "new@example.com",
            Password = "SecurePass123!",
            UserRole = "Participant"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(AuthController.Register), createdResult.ActionName);
    }

    [Fact]
    public async Task Register_EmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthenticationService>();
        var controller = new AuthController(mockAuthService.Object);

        var request = new RegisterRequest
        {
            Username = "",
            UserEmail = "test@example.com",
            Password = "SecurePass123!",
            UserRole = "Participant"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LoginResult { Success = true, UserId = 1, Role = "Participant", Message = "Login successful" });

        var controller = new AuthController(mockAuthService.Object);

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "SecurePass123!"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LoginResult { Success = false, Message = "Invalid username or password" });

        var controller = new AuthController(mockAuthService.Object);

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    #endregion

    #region Event Controller Tests

    [Fact]
    public async Task GetAllEvents_ReturnsListOfEvents()
    {
        // Arrange
        var db = GetDatabaseContext();
        
        // Add test data
        var track = new Track { TrackName = "Test Track", TrackLocation = "Test City", TrackGrade = "A" };
        db.Tracks.Add(track);
        await db.SaveChangesAsync();

        var @event = new Event { EventTitle = "Test Event", EventClass = "Class A", TrackID = track.TrackID };
        db.Events.Add(@event);
        await db.SaveChangesAsync();

        var controller = new EventsController(db);

        // Act
        var result = await controller.GetAllEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEvents = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Single(returnedEvents);
    }

    [Fact]
    public async Task GetEvent_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var db = GetDatabaseContext();

        var track = new Track { TrackName = "Test Track", TrackLocation = "Test City", TrackGrade = "A" };
        db.Tracks.Add(track);
        await db.SaveChangesAsync();

        var @event = new Event { EventTitle = "Test Event", EventClass = "Class A", TrackID = track.TrackID };
        db.Events.Add(@event);
        await db.SaveChangesAsync();

        var controller = new EventsController(db);

        // Act
        var result = await controller.GetEvent(@event.EventID);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetEvent_NonExistentEvent_ReturnsNotFound()
    {
        // Arrange
        var db = GetDatabaseContext();
        var controller = new EventsController(db);

        // Act
        var result = await controller.GetEvent(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    #endregion

    #region Team Controller Tests

    [Fact]
    public async Task GetAllTeams_ReturnsListOfTeams()
    {
        // Arrange
        var db = GetDatabaseContext();

        var user = new User { Username = "testuser", UserEmail = "test@example.com", PasswordHash = "hash", UserRole = "Participant" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var participant = new Participant { Partname = "Test Participant", PartCar = "Ferrari", UserID = user.UserID };
        db.Participants.Add(participant);
        await db.SaveChangesAsync();

        var team = new Team { TeamName = "Test Team", TeamPosition = "1", PartID = participant.PartID };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var controller = new TeamsController(db);

        // Act
        var result = await controller.GetAllTeams();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTeams = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Single(returnedTeams);
    }

    #endregion
}
