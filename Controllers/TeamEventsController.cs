using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

/// <summary>
/// Team-event registration management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeamEventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public TeamEventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all team-event registrations.
    /// </summary>
    /// <returns>List of all team-event records</returns>
    /// <response code="200">Team-events retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllTeamEvents()
    {
        var teamEvents = await _context.TeamEvents
            .Select(te => new { te.EventTeamID, te.TeamID, te.EventID })
            .ToListAsync();

        return Ok(teamEvents);
    }

    /// <summary>
    /// Retrieves a team-event record by EventTeamID.
    /// </summary>
    /// <param name="id">EventTeamID</param>
    /// <returns>Team-event details</returns>
    /// <response code="200">Team-event found</response>
    /// <response code="404">Team-event not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetTeamEvent(int id)
    {
        var teamEvent = await _context.TeamEvents
            .Where(te => te.EventTeamID == id)
            .Select(te => new { te.EventTeamID, te.TeamID, te.EventID })
            .FirstOrDefaultAsync();

        if (teamEvent == null)
            return NotFound(new { message = "Team-event not found" });

        return Ok(teamEvent);
    }

    /// <summary>
    /// Enrolls a team into an event.
    /// </summary>
    /// <param name="request">Team and event details</param>
    /// <returns>Created team-event record</returns>
    /// <response code="201">Team enrolled successfully</response>
    /// <response code="400">Invalid team or event</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CreateTeamEvent([FromBody] CreateTeamEventRequest request)
    {
        var teamExists = await _context.Teams.AnyAsync(t => t.TeamID == request.TeamID);
        if (!teamExists)
            return BadRequest(new { message = "Team does not exist" });

        var eventExists = await _context.Events.AnyAsync(e => e.EventID == request.EventID);
        if (!eventExists)
            return BadRequest(new { message = "Event does not exist" });

        var teamEvent = new TeamEvent
        {
            TeamID = request.TeamID,
            EventID = request.EventID
        };

        _context.TeamEvents.Add(teamEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeamEvent), new { id = teamEvent.EventTeamID },
            new { teamEvent.EventTeamID, teamEvent.TeamID, teamEvent.EventID });
    }

    /// <summary>
    /// Updates team or event assignment.
    /// </summary>
    /// <param name="id">EventTeamID</param>
    /// <param name="request">Updated team or event details</param>
    /// <returns>Updated team-event record</returns>
    /// <response code="200">Team-event updated successfully</response>
    /// <response code="404">Team-event not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> UpdateTeamEvent(int id, [FromBody] UpdateTeamEventRequest request)
    {
        var teamEvent = await _context.TeamEvents.FindAsync(id);
        if (teamEvent == null)
            return NotFound(new { message = "Team-event not found" });

        if (request.TeamID > 0)
        {
            var teamExists = await _context.Teams.AnyAsync(t => t.TeamID == request.TeamID);
            if (!teamExists)
                return BadRequest(new { message = "Team does not exist" });

            teamEvent.TeamID = request.TeamID;
        }

        if (request.EventID > 0)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == request.EventID);
            if (!eventExists)
                return BadRequest(new { message = "Event does not exist" });

            teamEvent.EventID = request.EventID;
        }

        _context.TeamEvents.Update(teamEvent);
        await _context.SaveChangesAsync();

        return Ok(new { teamEvent.EventTeamID, teamEvent.TeamID, teamEvent.EventID });
    }

    /// <summary>
    /// Withdraws a team from an event.
    /// </summary>
    /// <param name="id">EventTeamID</param>
    /// <returns>No content</returns>
    /// <response code="204">Team withdrawn successfully</response>
    /// <response code="404">Team-event not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTeamEvent(int id)
    {
        var teamEvent = await _context.TeamEvents.FindAsync(id);
        if (teamEvent == null)
            return NotFound(new { message = "Team-event not found" });

        _context.TeamEvents.Remove(teamEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTeamEventRequest
{
    public int TeamID { get; set; }
    public int EventID { get; set; }
}

public class UpdateTeamEventRequest
{
    public int TeamID { get; set; }
    public int EventID { get; set; }
}