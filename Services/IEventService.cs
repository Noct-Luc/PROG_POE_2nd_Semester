using RaceDay.Models;

namespace RaceDay.Services
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event> GetEventByIdAsync(int eventId);
        Task<Event> CreateEventAsync(string eventName, string eventClass, int? trackId);
        Task<Event> UpdateEventAsync(int eventId, string eventName, string eventClass, int? trackId);
        Task<bool> DeleteEventAsync(int eventId);
    }
}