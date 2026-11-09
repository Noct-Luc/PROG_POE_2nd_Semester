using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace RaceDay.MVC.Services;

/// <summary>
/// Sends requests to the API. The API authentication cookie is stored as a
/// protected claim in the current MVC authentication ticket and forwarded
/// per request; it is never stored in shared HttpClient default headers.
/// </summary>
public class ApiService : IApiService
{
    private const string ApiCookieClaim = "ApiAuthCookie";
    private const string ApiCookieName = "RaceDay.ApiAuth";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(
        HttpClient httpClient,
        ILogger<ApiService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<HttpResponseMessage> GetAsync(string endpoint) =>
        SendAsync(HttpMethod.Get, endpoint);

    public Task<HttpResponseMessage> PostAsync(string endpoint, object? content)
    {
        var json = JsonSerializer.Serialize(content);
        return SendAsync(HttpMethod.Post, endpoint,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }

    public async Task<HttpResponseMessage> PostWithFileAsync(
        string endpoint,
        IFormFile file,
        Dictionary<string, string>? additionalData = null)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var content = new MultipartFormDataContent();
        await using var fileStream = file.OpenReadStream();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        if (additionalData != null)
        {
            foreach (var item in additionalData)
                content.Add(new StringContent(item.Value), item.Key);
        }

        // SendAsync completes before the streams/content are disposed.
        return await SendAsync(HttpMethod.Post, endpoint, content);
    }

    public Task<HttpResponseMessage> PutAsync(string endpoint, object? content)
    {
        var json = JsonSerializer.Serialize(content);
        return SendAsync(HttpMethod.Put, endpoint,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }

    public Task<HttpResponseMessage> DeleteAsync(string endpoint) =>
        SendAsync(HttpMethod.Delete, endpoint);

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(method, endpoint)
        {
            Content = content
        };

        // Forward only the current user's API cookie. Do not use a shared
        // CookieContainer or DefaultRequestHeaders for per-user credentials.
        var apiCookie = _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ApiCookieClaim);

        if (!string.IsNullOrWhiteSpace(apiCookie))
            request.Headers.TryAddWithoutValidation(
                "Cookie", $"{ApiCookieName}={apiCookie}");

        try
        {
            _logger.LogInformation("{Method} request to {Endpoint}", method, endpoint);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "{Method} request to {Endpoint} returned {StatusCode}",
                    method, endpoint, response.StatusCode);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during {Method} request to {Endpoint}",
                method, endpoint);
            throw;
        }
    }
}
