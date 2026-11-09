using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Text.Json;

namespace RaceDay.MVC.Controllers;

/// <summary>
/// Dashboard controller for organizers (Managers) to manage events and view statistics.
/// </summary>
[Authorize(Policy = "Manager")]
public class OrganizerDashboardController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<OrganizerDashboardController> _logger;
    private readonly IConfiguration _configuration;

    public OrganizerDashboardController(IApiService apiService, ILogger<OrganizerDashboardController> logger, IConfiguration configuration)
    {
        _apiService = apiService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Displays the organizer dashboard with event summary and statistics.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _apiService.GetAsync("api/events");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API returned error: {StatusCode}", response.StatusCode);
                return View(new OrganizerDashboardViewModel());
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<EventDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var dashboard = new OrganizerDashboardViewModel
            {
                TotalEvents = events?.Count ?? 0,
                Events = events ?? new List<EventDto>(),
                
            };

            return View(dashboard);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error loading organizer dashboard.");
            TempData["ErrorMessage"] = "Unable to connect to the API. Please try again later.";
            return View(new OrganizerDashboardViewModel());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing API response.");
            TempData["ErrorMessage"] = "Error processing data from server.";
            return View(new OrganizerDashboardViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading organizer dashboard.");
            TempData["ErrorMessage"] = "An unexpected error occurred.";
            return View(new OrganizerDashboardViewModel());
        }
    }

    /// <summary>
    /// Displays the events management page.
    /// </summary>
    public async Task<IActionResult> ManageEvents()
    {
        try
        {
            var response = await _apiService.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API returned error: {StatusCode}", response.StatusCode);
                return View(new List<EventDto>());
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<EventDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(events ?? new List<EventDto>());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error loading events management page.");
            TempData["ErrorMessage"] = "Unable to connect to the API. Please try again later.";
            return View(new List<EventDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events management page.");
            TempData["ErrorMessage"] = "An error occurred while loading events.";
            return View(new List<EventDto>());
        }
    }

    /// <summary>
    /// Displays the create event form.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateEvent()
    {
        try
        {
            var response = await _apiService.GetAsync("api/tracks");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API returned error: {StatusCode}", response.StatusCode);
                return View(new CreateEventViewModel());
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var tracks = JsonSerializer.Deserialize<List<TrackDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var model = new CreateEventViewModel
            {
                Tracks = tracks ?? new List<TrackDto>()
            };

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error loading create event form.");
            TempData["ErrorMessage"] = "Unable to connect to the API. Please try again later.";
            return View(new CreateEventViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create event form.");
            TempData["ErrorMessage"] = "An error occurred while loading the form.";
            return View(new CreateEventViewModel());
        }
    }

    /// <summary>
    /// Submits a new event to the API with optional banner image.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload tracks for form redisplay
            try
            {
                var response = await _apiService.GetAsync("api/tracks");
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var tracks = JsonSerializer.Deserialize<List<TrackDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    model.Tracks = tracks ?? new List<TrackDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading tracks for form.");
            }
            return View(model);
        }

        try
        {
            var createRequest = new
            {
                EventTitle = model.EventTitle,
                EventClass = model.EventType,
                TrackID = model.TrackID
            };

            var response = await _apiService.PostAsync("api/events", createRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                _logger.LogError(
                    "Failed to create event. Status: {StatusCode}. Response: {Error}",
                    response.StatusCode,
                    errorContent
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"Failed to create event: {errorContent}"
                );

                return View(model);
            }

            // Extract event ID from response
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdEvent = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Upload banner image if provided
            if (model.BannerImage != null && model.BannerImage.Length > 0 && createdEvent?.EventID > 0)
            {
                await UploadEventBannerAsync(createdEvent.EventID, model.BannerImage);
            }

            TempData["SuccessMessage"] = "Event created successfully!";
            return RedirectToAction(nameof(ManageEvents));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error creating event.");
            ModelState.AddModelError(string.Empty, "Unable to connect to the API. Please try again later.");
            return View(model);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing API response.");
            ModelState.AddModelError(string.Empty, "Error processing server response.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating event.");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Displays the enrolments for a specific event.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ViewEnrolments(int eventId)
    {
        try
        {
            var eventResponse = await _apiService.GetAsync($"api/events/{eventId}");
            
            if (!eventResponse.IsSuccessStatusCode)
            {
                _logger.LogError("API returned error for event {EventId}: {StatusCode}", eventId, eventResponse.StatusCode);
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(ManageEvents));
            }

            var eventContent = await eventResponse.Content.ReadAsStringAsync();
            var @event = JsonSerializer.Deserialize<EventDto>(eventContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var enrollmentsResponse = await _apiService.GetAsync($"api/events/{eventId}/enrolments");
            var enrolments = new List<EnrolmentDto>();
            
            if (enrollmentsResponse.IsSuccessStatusCode)
            {
                var enrollmentsContent = await enrollmentsResponse.Content.ReadAsStringAsync();
                enrolments = JsonSerializer.Deserialize<List<EnrolmentDto>>(enrollmentsContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EnrolmentDto>();
            }

            var model = new EnrolmentsViewModel
            {
                Event = @event,
                Enrolments = enrolments
            };

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error loading enrolments for event {EventId}.", eventId);
            TempData["ErrorMessage"] = "Unable to connect to the API.";
            return RedirectToAction(nameof(ManageEvents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading enrolments for event {EventId}.", eventId);
            TempData["ErrorMessage"] = "An error occurred while loading enrolments.";
            return RedirectToAction(nameof(ManageEvents));
        }
    }

    /// <summary>
    /// Displays the results entry page for an event.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EnterResults(int eventId)
    {
        try
        {
            var enrollmentsResponse = await _apiService.GetAsync($"api/events/{eventId}/enrolments");
            
            if (!enrollmentsResponse.IsSuccessStatusCode)
            {
                _logger.LogError("API returned error for event {EventId}: {StatusCode}", eventId, enrollmentsResponse.StatusCode);
                TempData["ErrorMessage"] = "Event or enrolments not found.";
                return RedirectToAction(nameof(ManageEvents));
            }

            var jsonContent = await enrollmentsResponse.Content.ReadAsStringAsync();
            var enrolments = JsonSerializer.Deserialize<List<EnrolmentDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var model = new ResultsEntryViewModel
            {
                EventID = eventId,
                Enrolments = enrolments ?? new List<EnrolmentDto>()
            };

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error loading results entry for event {EventId}.", eventId);
            TempData["ErrorMessage"] = "Unable to connect to the API.";
            return RedirectToAction(nameof(ManageEvents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading results entry for event {EventId}.", eventId);
            TempData["ErrorMessage"] = "An error occurred.";
            return RedirectToAction(nameof(ManageEvents));
        }
    }

    /// <summary>
    /// Uploads a banner image for an event through the API.
    /// </summary>
    private async Task<bool> UploadEventBannerAsync(int eventId, IFormFile bannerImage)
    {
        try
        {
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSizeBytes", 5_242_880);
            var allowedExtensionsConfig = _configuration.GetValue<string>("FileUpload:AllowedExtensions", ".jpg,.jpeg,.png,.gif");
            var allowedExtensions = allowedExtensionsConfig?.Split(',').ToList() ?? new List<string>();

            // Validate file size
            if (bannerImage.Length > maxFileSize)
            {
                _logger.LogWarning("Banner image for event {EventId} exceeds maximum file size.", eventId);
                return false;
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(bannerImage.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Banner image for event {EventId} has invalid file type: {FileType}.", eventId, fileExtension);
                return false;
            }

            var additionalData = new Dictionary<string, string>
            {
                { "eventId", eventId.ToString() }
            };

            var response = await _apiService.PostWithFileAsync("api/events/upload-banner", bannerImage, additionalData);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Banner image uploaded successfully for event {EventId}.", eventId);
                return true;
            }

            _logger.LogError("Failed to upload banner image for event {EventId}.", response.StatusCode, eventId);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection error uploading banner image for event {EventId}.", eventId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading banner image for event {EventId}.", eventId);
            return false;
        }
    }
}

