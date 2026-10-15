using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

/// <summary>
/// Racing event management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all scheduled racing events.
    /// </summary>
    /// <returns>List of all events</returns>
    /// <response code="200">Events retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllEvents()
    {
        var events = await _context.Events
            .Select(e => new { e.EventID, e.EventTitle, e.EventClass, e.TrackID })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>
    /// Retrieves specific event details.
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Event details</returns>
    /// <response code="200">Event found</response>
    /// <response code="404">Event not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetEvent(int id)
    {
        var @event = await _context.Events
            .Where(e => e.EventID == id)
            .Select(e => new { e.EventID, e.EventTitle, e.EventClass, e.TrackID })
            .FirstOrDefaultAsync();

        if (@event == null)
            return NotFound(new { message = "Event not found" });

        return Ok(@event);
    }

    /// <summary>
    /// Creates a new racing event.
    /// </summary>
    /// <param name="request">Event details</param>
    /// <returns>Created event</returns>
    /// <response code="201">Event created successfully</response>
    /// <response code="400">Invalid event details</response>
    [HttpPost]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CreateEvent([FromBody] CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventTitle))
            return BadRequest(new { message = "Event name is required" });

        var trackExists = await _context.Tracks.AnyAsync(t => t.TrackID == request.TrackID);
        if (!trackExists)
            return BadRequest(new { message = "Track does not exist" });

        var @event = new Event
        {
            EventTitle = request.EventTitle,
            EventClass = request.EventClass ?? string.Empty,
            TrackID = request.TrackID
        };

        _context.Events.Add(@event);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = @event.EventID },
            new { @event.EventID, @event.EventTitle, @event.EventClass, @event.TrackID });
    }

    /// <summary>
    /// Updates event details or assigned track.
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="request">Updated event details</param>
    /// <returns>Updated event</returns>
    /// <response code="200">Event updated successfully</response>
    /// <response code="404">Event not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event == null)
            return NotFound(new { message = "Event not found" });

        if (!string.IsNullOrWhiteSpace(request.EventTitle))
            @event.EventTitle = request.EventTitle;

        if (!string.IsNullOrWhiteSpace(request.EventClass))
            @event.EventClass = request.EventClass;

        if (request.TrackID > 0)
        {
            var trackExists = await _context.Tracks.AnyAsync(t => t.TrackID == request.TrackID);
            if (!trackExists)
                return BadRequest(new { message = "Track does not exist" });

            @event.TrackID = request.TrackID;
        }

        _context.Events.Update(@event);
        await _context.SaveChangesAsync();

        return Ok(new { @event.EventID, @event.EventTitle, @event.EventClass, @event.TrackID });
    }

    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Event deleted successfully</response>
    /// <response code="404">Event not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteEvent(int id)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event == null)
            return NotFound(new { message = "Event not found" });

        _context.Events.Remove(@event);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateEventRequest
{
    public string EventTitle { get; set; } = string.Empty;
    public string? EventClass { get; set; }
    public int TrackID { get; set; }
}

public class UpdateEventRequest
{
    public string? EventTitle { get; set; }
    public string? EventClass { get; set; }
    public int TrackID { get; set; }
}