using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Security.Claims;
using System.Text.Json;

namespace RaceDay.MVC.Controllers;

/// <summary>
/// Dashboard controller for participants to view events, enroll, and track results.
/// </summary>
[Authorize(Policy = "Participant")]
public class ParticipantDashboardController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<ParticipantDashboardController> _logger;

    public ParticipantDashboardController(IApiService apiService, ILogger<ParticipantDashboardController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the participant dashboard.
    /// </summary>


    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var response = await _apiService.GetAsync(
                $"api/participants/{userId}/enrollments");

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();

            var enrollments =
                JsonSerializer.Deserialize<List<EnrollmentDisplayDto>>(
                    jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<EnrollmentDisplayDto>();

            var dashboard = new ParticipantDashboardViewModel
            {
                TotalEnrollments = enrollments.Count,

                Enrollments = enrollments,

                CompletedEvents = enrollments.Count(e =>
                    string.Equals(
                        e.Status,
                        "Completed",
                        StringComparison.OrdinalIgnoreCase))
            };

            return View(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading participant dashboard.");

            return View(new ParticipantDashboardViewModel());
        }
    }



    /// <summary>
    /// Displays all available events for enrollment.
    /// </summary>
    public async Task<IActionResult> BrowseEvents(
    string? searchTerm = null,
    string? filterType = null)
    {
        try
        {
            var response = await _apiService.GetAsync("api/events");

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();

            var allEvents =
                JsonSerializer.Deserialize<List<EventDto>>(
                    jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            var events = allEvents ?? new List<EventDto>();

            // Search by event title.
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                events = events
                    .Where(e => e.EventTitle.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by event class.
            if (!string.IsNullOrWhiteSpace(filterType))
            {
                events = events
                    .Where(e => e.EventClass.Equals(
                        filterType,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events for browsing.");

            return View(new List<EventDto>());
        }
    }

    /// <summary>
    /// Displays detailed information about a specific event.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EventDetails(int eventId)
    {
        try
        {
            var response = await _apiService.GetAsync($"api/events/{eventId}");
            var jsonContent = await response.Content.ReadAsStringAsync();
            var @event = JsonSerializer.Deserialize<EventDto>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var categoriesResponse = await _apiService.GetAsync($"api/events/{eventId}/categories");
            var categoriesJsonContent = await categoriesResponse.Content.ReadAsStringAsync();
            var categories = JsonSerializer.Deserialize<List<CategoryDto>>(categoriesJsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var model = new EventDetailsViewModel
            {
                Event = @event,
                Categories = categories ?? new List<CategoryDto>()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event details for event {EventId}.", eventId);
            return NotFound();
        }
    }

    /// <summary>
    /// Enrolls a participant in an event.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnrollEvent(int eventId, int categoryId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var enrollRequest = new
            {
                UserId = userId,
                EventId = eventId,
                CategoryId = categoryId
            };

            var response = await _apiService.PostAsync("api/enrollments", enrollRequest);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to enroll in event. Please try again.";
                return RedirectToAction(nameof(EventDetails), new { eventId });
            }

            TempData["SuccessMessage"] = "Successfully enrolled in the event!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling participant in event {EventId}.", eventId);
            TempData["ErrorMessage"] = "An error occurred during enrollment.";
            return RedirectToAction(nameof(EventDetails), new { eventId });
        }
    }

    /// <summary>
    /// Displays the participant's personal race results.
    /// </summary>
    public async Task<IActionResult> MyResults()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var resultsResponse = await _apiService.GetAsync($"api/participants/{userId}/results");
            var jsonContent = await resultsResponse.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<List<ParticipantResultDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(results ?? new List<ParticipantResultDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading participant results.");
            return View(new List<ParticipantResultDto>());
        }
    }

    /// <summary>
    /// Displays and allows updating participant profile information.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var response = await _apiService.GetAsync($"api/users/{userId}");
            var jsonContent = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserProfileDto>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading participant profile.");
            return View();
        }
    }

    /// <summary>
    /// Updates participant profile information.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(UserProfileDto model)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var updateRequest = new
            {
                model.Username,
                model.UserEmail
            };

            var response = await _apiService.PutAsync($"api/users/{userId}", updateRequest);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to update profile.");
                return View(nameof(Profile), model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant profile.");
            ModelState.AddModelError(string.Empty, "An error occurred while updating profile.");
            return View(nameof(Profile), model);
        }
    }

    /// <summary>
    /// Retrieves all events associated with a participant through their teams.
    /// </summary>

    /// <summary>
    /// Retrieves all events associated with a participant through their teams.
    /// </summary>


}
public class ParticipantEnrollmentResponse
{
    public int EnrolmentID { get; set; }

    public int EventID { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public DateTime? EventDate { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}