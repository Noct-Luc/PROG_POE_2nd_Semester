namespace RaceDay.MVC.Services;

/// <summary>Service for communicating with the RaceDay API.</summary>
public interface IApiService
{
    Task<HttpResponseMessage> GetAsync(string endpoint);
    Task<HttpResponseMessage> PostAsync(string endpoint, object? content);
    Task<HttpResponseMessage> PostWithFileAsync(
        string endpoint,
        IFormFile file,
        Dictionary<string, string>? additionalData = null);
    Task<HttpResponseMessage> PutAsync(string endpoint, object? content);
    Task<HttpResponseMessage> DeleteAsync(string endpoint);
}
