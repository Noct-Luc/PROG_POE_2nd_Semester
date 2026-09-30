using RaceDay.Data;
using RaceDay.Models;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Services
{
    public class EventService : IEventService
    {
        private readonly RaceDayDbContext _context;

        public EventService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events.Include(e => e.Track).Include(e => e.TeamEvents).ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(int eventId)
        {
            return await _context.Events
                .Include(e => e.Track)
                .Include(e => e.TeamEvents)
                .FirstOrDefaultAsync(e => e.EventID == eventId);
        }

        public async Task<Event> CreateEventAsync(string eventName, string eventClass, int? trackId)
        {
            var @event = new Event
            {
                EventName = eventName,
                EventClass = eventClass,
                TrackID = trackId
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task<Event> UpdateEventAsync(int eventId, string eventName, string eventClass, int? trackId)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
            if (@event == null)
                return null;

            @event.EventName = eventName;
            @event.EventClass = eventClass;
            @event.TrackID = trackId;

            _context.Events.Update(@event);
            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
            if (@event == null)
                return false;

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}