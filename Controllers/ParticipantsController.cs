using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

/// <summary>
/// Participant management endpoints for race event participants.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ParticipantsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all registered participants.
    /// </summary>
    /// <returns>List of all participants</returns>
    /// <response code="200">Participants retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllParticipants()
    {
        var participants = await _context.Participants
            .Select(p => new { p.PartID, p.Partname, p.PartCar, p.PartWins, p.UserID })
            .ToListAsync();

        return Ok(participants);
    }

    /// <summary>
    /// Retrieves a specific participant by ID.
    /// </summary>
    /// <param name="id">Participant ID</param>
    /// <returns>Participant details</returns>
    /// <response code="200">Participant found</response>
    /// <response code="404">Participant not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetParticipant(int id)
    {
        var participant = await _context.Participants
            .Where(p => p.PartID == id)
            .Select(p => new { p.PartID, p.Partname, p.PartCar, p.PartWins, p.UserID })
            .FirstOrDefaultAsync();

        if (participant == null)
            return NotFound(new { message = "Participant not found" });

        return Ok(participant);
    }

    /// <summary>
    /// Registers a new participant.
    /// </summary>
    /// <param name="request">Participant details</param>
    /// <returns>Created participant</returns>
    /// <response code="201">Participant registered successfully</response>
    /// <response code="400">Invalid participant details</response>
    [HttpPost]
    [Authorize(Policy = "Participant")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CreateParticipant([FromBody] CreateParticipantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Partname) || string.IsNullOrWhiteSpace(request.PartCar))
            return BadRequest(new { message = "Participant name and car are required" });

        var participant = new Participant
        {
            Partname = request.Partname,
            PartCar = request.PartCar,
            PartWins = request.PartWins,
            UserID = request.UserID
        };

        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetParticipant), new { id = participant.PartID },
            new { participant.PartID, participant.Partname, participant.PartCar, participant.PartWins, participant.UserID });
    }

    /// <summary>
    /// Updates participant details and wins.
    /// </summary>
    /// <param name="id">Participant ID</param>
    /// <param name="request">Updated participant details</param>
    /// <returns>Updated participant</returns>
    /// <response code="200">Participant updated successfully</response>
    /// <response code="404">Participant not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> UpdateParticipant(int id, [FromBody] UpdateParticipantRequest request)
    {
        var participant = await _context.Participants.FindAsync(id);
        if (participant == null)
            return NotFound(new { message = "Participant not found" });

        if (!string.IsNullOrWhiteSpace(request.Partname))
            participant.Partname = request.Partname;

        if (!string.IsNullOrWhiteSpace(request.PartCar))
            participant.PartCar = request.PartCar;

        if (request.PartWins >= 0)
            participant.PartWins = request.PartWins;

        _context.Participants.Update(participant);
        await _context.SaveChangesAsync();

        return Ok(new { participant.PartID, participant.Partname, participant.PartCar, participant.PartWins, participant.UserID });
    }

    /// <summary>
    /// Removes a participant.
    /// </summary>
    /// <param name="id">Participant ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Participant deleted successfully</response>
    /// <response code="404">Participant not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteParticipant(int id)
    {
        var participant = await _context.Participants.FindAsync(id);
        if (participant == null)
            return NotFound(new { message = "Participant not found" });

        _context.Participants.Remove(participant);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateParticipantRequest
{
    public string Partname { get; set; } = string.Empty;
    public string PartCar { get; set; } = string.Empty;
    public int PartWins { get; set; }
    public int UserID { get; set; }
}

public class UpdateParticipantRequest
{
    public string? Partname { get; set; }
    public string? PartCar { get; set; }
    public int PartWins { get; set; }
}