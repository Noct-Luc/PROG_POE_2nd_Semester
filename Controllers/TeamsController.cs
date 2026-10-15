using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

/// <summary>
/// Racing team management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public TeamsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all racing teams.
    /// </summary>
    /// <returns>List of all teams</returns>
    /// <response code="200">Teams retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllTeams()
    {
        var teams = await _context.Teams
            .Select(t => new { t.TeamID, t.TeamName, t.TeamPosition, t.PartID })
            .ToListAsync();

        return Ok(teams);
    }

    /// <summary>
    /// Retrieves team details by TeamID.
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>Team details</returns>
    /// <response code="200">Team found</response>
    /// <response code="404">Team not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetTeam(int id)
    {
        var team = await _context.Teams
            .Where(t => t.TeamID == id)
            .Select(t => new { t.TeamID, t.TeamName, t.TeamPosition, t.PartID })
            .FirstOrDefaultAsync();

        if (team == null)
            return NotFound(new { message = "Team not found" });

        return Ok(team);
    }

    /// <summary>
    /// Creates a new racing team.
    /// </summary>
    /// <param name="request">Team details</param>
    /// <returns>Created team</returns>
    /// <response code="201">Team created successfully</response>
    /// <response code="400">Invalid team details</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CreateTeam([FromBody] CreateTeamRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TeamName))
            return BadRequest(new { message = "Team name is required" });

        var participantExists = await _context.Participants.AnyAsync(p => p.PartID == request.PartID);
        if (!participantExists)
            return BadRequest(new { message = "Participant does not exist" });

        var team = new Team
        {
            TeamName = request.TeamName,
            TeamPosition = request.TeamPosition ?? string.Empty,
            PartID = request.PartID
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeam), new { id = team.TeamID },
            new { team.TeamID, team.TeamName, team.TeamPosition, team.PartID });
    }

    /// <summary>
    /// Updates team name or ranking position.
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <param name="request">Updated team details</param>
    /// <returns>Updated team</returns>
    /// <response code="200">Team updated successfully</response>
    /// <response code="404">Team not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> UpdateTeam(int id, [FromBody] UpdateTeamRequest request)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        if (!string.IsNullOrWhiteSpace(request.TeamName))
            team.TeamName = request.TeamName;

        if (!string.IsNullOrWhiteSpace(request.TeamPosition))
            team.TeamPosition = request.TeamPosition;

        _context.Teams.Update(team);
        await _context.SaveChangesAsync();

        return Ok(new { team.TeamID, team.TeamName, team.TeamPosition, team.PartID });
    }

    /// <summary>
    /// Deletes a team record.
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Team deleted successfully</response>
    /// <response code="404">Team not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTeamRequest
{
    public string TeamName { get; set; } = string.Empty;
    public string? TeamPosition { get; set; }
    public int PartID { get; set; }
}

public class UpdateTeamRequest
{
    public string? TeamName { get; set; }
    public string? TeamPosition { get; set; }
    public int PartID { get; set; }
}