using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Security.Claims;
using System.Text.Json;

namespace RaceDay.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<AccountController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AccountController(
        IApiService apiService,
        ILogger<AccountController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // LOGIN GET
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // LOGIN POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var loginRequest = new
            {
                model.Username,
                model.Password
            };

            var response = await _apiService.PostAsync(
                "api/auth/login",
                loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody =
                    await response.Content.ReadAsStringAsync();

                var errorMessage = ExtractApiError(responseBody);

                _logger.LogWarning(
                    "API login failed. Status: {StatusCode}. Message: {Message}",
                    response.StatusCode,
                    errorMessage);

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                return View(model);
            }

            var jsonContent =
                await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<LoginResponse>(
                jsonContent,
                JsonOptions);

            if (result == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid response received from the server.");

                return View(model);
            }

            // The API issues its own authentication cookie. Capture its value
            // and keep it inside the encrypted MVC authentication ticket so
            // ApiService can forward it for this user on protected API calls.
            var apiCookie = ExtractApiCookie(response);
            if (string.IsNullOrWhiteSpace(apiCookie))
            {
                _logger.LogError("API login succeeded but no API authentication cookie was returned.");
                ModelState.AddModelError(
                    string.Empty,
                    "The server did not establish an API session. Please try again.");
                return View(model);
            }

            var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
    new(ClaimTypes.Name, result.Username),
    new("UserRole", result.UserRole),
    new("ApiAuthCookie", apiCookie)
};
            if (string.IsNullOrWhiteSpace(result.UserRole) ||
    (result.UserRole != "Manager" &&
     result.UserRole != "Participant"))
            {
                _logger.LogError(
                    "Invalid role received from API: {Role}",
                    result.UserRole);

                ModelState.AddModelError(
                    string.Empty,
                    "Your account has an invalid role. Please contact the administrator.");

                return View(model);
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties);

            _logger.LogInformation(
                "User {Username} logged in successfully.",
                model.Username);

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return result.UserRole == "Manager"
    ? RedirectToAction("Index", "OrganizerDashboard")
    : RedirectToAction("Index", "ParticipantDashboard");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error during login.");

            ModelState.AddModelError(
                string.Empty,
                "Unable to connect to the API. Please check that the API is running.");

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected login error.");

            ModelState.AddModelError(
                string.Empty,
                "An unexpected error occurred. Please try again.");

            return View(model);
        }
    }

    // REGISTRATION GET
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    // REGISTRATION POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model)
    {
        _logger.LogInformation(
            "Registration received. Username: {Username}, Email: {Email}, Role: {Role}",
            model.Username,
            model.UserEmail,
            model.UserRole);

        // MVC validation
        if (!ModelState.IsValid)
        {
            LogRegistrationErrors(model.Username);

            return View("Register", model);
        }

        try
        {
            // IMPORTANT:
            // The API expects Password, NOT PasswordHash.
            // Also validate UserRole before sending to API
            if (string.IsNullOrEmpty(model.UserRole))
            {
                ModelState.AddModelError(
                    nameof(model.UserRole),
                    "Account type is required.");
                return View("Register", model);
            }

            // Public self-registration is restricted to Participant accounts.
            // Manager accounts must be provisioned through a trusted process.
            if (model.UserRole != "Participant")
            {
                ModelState.AddModelError(
                    nameof(model.UserRole),
                    "Public registration is only available for Participant accounts.");
                return View("Register", model);
            }

            var registerRequest = new
            {
                Username = model.Username.Trim(),
                UserEmail = model.UserEmail.Trim(),
                Password = model.Password,
                UserRole = model.UserRole
            };

            _logger.LogInformation(
                "Sending registration request to API. UserRole: {UserRole}",
                model.UserRole);

            var response = await _apiService.PostAsync(
                "api/auth/register",
                registerRequest);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            // API registration failure
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage =
                    ExtractApiError(responseBody);

                _logger.LogWarning(
                    "Registration API failure. Status: {StatusCode}. Message: {Message}. Request: Username={Username}, UserRole={UserRole}",
                    (int)response.StatusCode,
                    errorMessage,
                    model.Username,
                    model.UserRole);

                ModelState.AddModelError(
                    string.Empty,
                    $"Registration failed: {errorMessage}");

                LogRegistrationErrors(model.Username);

                return View("Register", model);
            }

            // Registration successful
            _logger.LogInformation(
                "Registration completed successfully for {Username}.",
                model.Username);

            TempData["SuccessMessage"] =
                "Registration successful! Please log in.";

            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "API connection error during registration.");

            ModelState.AddModelError(
                string.Empty,
                "Unable to connect to the API. Please check that the API is running.");

            return View("Register", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected registration error.");

            ModelState.AddModelError(
                string.Empty,
                "An unexpected registration error occurred. Please try again.");

            return View("Register", model);
        }
    }

    // PROFILE GET
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public IActionResult Profile()
    {
        return View();
    }

    // SETTINGS GET
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public IActionResult Settings()
    {
        return View();
    }

    // ACCESS DENIED
    public IActionResult AccessDenied()
    {
        return View();
    }

    // LOGOUT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Expire the API session as well as the MVC session.
        try
        {
            await _apiService.PostAsync("api/auth/logout", null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not call API logout; clearing the MVC session anyway.");
        }

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    private static string? ExtractApiCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            return null;

        foreach (var header in setCookieHeaders)
        {
            const string cookiePrefix = "RaceDay.ApiAuth=";
            var start = header.IndexOf(cookiePrefix, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                continue;

            start += cookiePrefix.Length;
            var end = header.IndexOf(';', start);
            return end >= 0
                ? header[start..end]
                : header[start..];
        }

        return null;
    }

    // Extracts errors from ordinary JSON and ASP.NET validation responses.
    private static string ExtractApiError(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return "The API returned an empty error response.";

        try
        {
            using var document = JsonDocument.Parse(responseBody);

            var root = document.RootElement;

            if (root.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String)
            {
                return message.GetString()
                    ?? "Registration request was rejected.";
            }

            if (root.TryGetProperty("title", out var title) &&
                title.ValueKind == JsonValueKind.String)
            {
                return title.GetString()
                    ?? "The submitted information was invalid.";
            }

            if (root.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Object)
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(property =>
                        property.Value.ValueKind == JsonValueKind.Array
                            ? property.Value.EnumerateArray()
                                .Where(item => item.ValueKind == JsonValueKind.String)
                                .Select(item => item.GetString()!)
                            : Enumerable.Empty<string>())
                    .ToList();

                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }
        }
        catch (JsonException)
        {
            // Response was not valid JSON.
        }

        return "The API rejected the request. Please check your submitted information.";
    }

    private void LogRegistrationErrors(string username)
    {
        var errors = string.Join(
            "; ",
            ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage));

        _logger.LogWarning(
            "Registration validation failed for {Username}. Errors: {Errors}",
            username,
            errors);
    }
}
