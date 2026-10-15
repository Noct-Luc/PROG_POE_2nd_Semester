using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

/// <summary>
/// Race track management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public TracksController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all race tracks.
    /// </summary>
    /// <returns>List of all tracks</returns>
    /// <response code="200">Tracks retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllTracks()
    {
        var tracks = await _context.Tracks
            .Select(t => new { t.TrackID, t.TrackName, t.TrackLocation, t.TrackGrade })
            .ToListAsync();

        return Ok(tracks);
    }

    /// <summary>
    /// Retrieves track details by TrackID.
    /// </summary>
    /// <param name="id">Track ID</param>
    /// <returns>Track details</returns>
    /// <response code="200">Track found</response>
    /// <response code="404">Track not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetTrack(int id)
    {
        var track = await _context.Tracks
            .Where(t => t.TrackID == id)
            .Select(t => new { t.TrackID, t.TrackName, t.TrackLocation, t.TrackGrade })
            .FirstOrDefaultAsync();

        if (track == null)
            return NotFound(new { message = "Track not found" });

        return Ok(track);
    }

    /// <summary>
    /// Adds a new race track.
    /// </summary>
    /// <param name="request">Track details</param>
    /// <returns>Created track</returns>
    /// <response code="201">Track created successfully</response>
    /// <response code="400">Invalid track details</response>
    [HttpPost]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CreateTrack([FromBody] CreateTrackRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TrackName) || string.IsNullOrWhiteSpace(request.TrackLocation))
            return BadRequest(new { message = "Track name and location are required" });

        var track = new Track
        {
            TrackName = request.TrackName,
            TrackLocation = request.TrackLocation,
            TrackGrade = request.TrackGrade ?? string.Empty
        };

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrack), new { id = track.TrackID },
            new { track.TrackID, track.TrackName, track.TrackLocation, track.TrackGrade });
    }

    /// <summary>
    /// Updates track information.
    /// </summary>
    /// <param name="id">Track ID</param>
    /// <param name="request">Updated track details</param>
    /// <returns>Updated track</returns>
    /// <response code="200">Track updated successfully</response>
    /// <response code="404">Track not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> UpdateTrack(int id, [FromBody] UpdateTrackRequest request)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null)
            return NotFound(new { message = "Track not found" });

        if (!string.IsNullOrWhiteSpace(request.TrackName))
            track.TrackName = request.TrackName;

        if (!string.IsNullOrWhiteSpace(request.TrackLocation))
            track.TrackLocation = request.TrackLocation;

        if (!string.IsNullOrWhiteSpace(request.TrackGrade))
            track.TrackGrade = request.TrackGrade;

        _context.Tracks.Update(track);
        await _context.SaveChangesAsync();

        return Ok(new { track.TrackID, track.TrackName, track.TrackLocation, track.TrackGrade });
    }

    /// <summary>
    /// Removes a track.
    /// </summary>
    /// <param name="id">Track ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Track deleted successfully</response>
    /// <response code="404">Track not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTrack(int id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null)
            return NotFound(new { message = "Track not found" });

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTrackRequest
{
    public string TrackName { get; set; } = string.Empty;
    public string TrackLocation { get; set; } = string.Empty;
    public string? TrackGrade { get; set; }
}

public class UpdateTrackRequest
{
    public string? TrackName { get; set; }
    public string? TrackLocation { get; set; }
    public string? TrackGrade { get; set; }
}