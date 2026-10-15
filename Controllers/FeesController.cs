    using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers;

/// <summary>
/// Fee payment management endpoints for race event fees.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public FeesController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all fee payment records.
    /// </summary>
    /// <returns>List of all fee records</returns>
    /// <response code="200">Fees retrieved successfully</response>
    /// <response code="403">Only managers can view all fees</response>
    [HttpGet]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllFees()
    {
        var fees = await _context.Fees
            .Select(f => new { f.FeeID, f.FeePaid, f.FeeDiscount, f.FeeAmount, f.PartID })
            .ToListAsync();

        return Ok(fees);
    }

    /// <summary>
    /// Retrieves fee details by FeeID.
    /// </summary>
    /// <param name="id">Fee ID</param>
    /// <returns>Fee details</returns>
    /// <response code="200">Fee found</response>
    /// <response code="404">Fee not found</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetFee(int id)
    {
        var fee = await _context.Fees
            .Where(f => f.FeeID == id)
            .Select(f => new { f.FeeID, f.FeePaid, f.FeeDiscount, f.FeeAmount, f.PartID })
            .FirstOrDefaultAsync();

        if (fee == null)
            return NotFound(new { message = "Fee not found" });

        return Ok(fee);
    }

    /// <summary>
    /// Retrieves fee details for a specific participant.
    /// </summary>
    /// <param name="partId">Participant ID</param>
    /// <returns>Fee details for the participant</returns>
    /// <response code="200">Fee found</response>
    /// <response code="404">Fee not found</response>
    [HttpGet("participant/{partId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetFeeByParticipant(int partId)
    {
        var fee = await _context.Fees
            .Where(f => f.PartID == partId)
            .Select(f => new { f.FeeID, f.FeePaid, f.FeeDiscount, f.FeeAmount, f.PartID })
            .FirstOrDefaultAsync();

        if (fee == null)
            return NotFound(new { message = "Fee record not found for this participant" });

        return Ok(fee);
    }

    /// <summary>
    /// Records a new fee payment entry.
    /// </summary>
    /// <param name="request">Fee payment details</param>
    /// <returns>Created fee record</returns>
    /// <response code="201">Fee recorded successfully</response>
    /// <response code="400">Invalid fee details</response>
    [HttpPost]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CreateFee([FromBody] CreateFeeRequest request)
    {
        if (request.FeeAmount <= 0)
            return BadRequest(new { message = "Fee amount must be greater than zero" });

        var participantExists = await _context.Participants.AnyAsync(p => p.PartID == request.PartID);
        if (!participantExists)
            return BadRequest(new { message = "Participant does not exist" });

        var fee = new Fee
        {
            FeePaid = request.FeePaid ?? "No",
            FeeDiscount = request.FeeDiscount ?? "No",
            FeeAmount = request.FeeAmount,
            PartID = request.PartID
        };

        _context.Fees.Add(fee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFee), new { id = fee.FeeID },
            new { fee.FeeID, fee.FeePaid, fee.FeeDiscount, fee.FeeAmount, fee.PartID });
    }

    /// <summary>
    /// Updates fee payment status or amount.
    /// </summary>
    /// <param name="id">Fee ID</param>
    /// <param name="request">Updated fee details</param>
    /// <returns>Updated fee record</returns>
    /// <response code="200">Fee updated successfully</response>
    /// <response code="404">Fee not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> UpdateFee(int id, [FromBody] UpdateFeeRequest request)
    {
        var fee = await _context.Fees.FindAsync(id);
        if (fee == null)
            return NotFound(new { message = "Fee not found" });

        if (!string.IsNullOrWhiteSpace(request.FeePaid))
            fee.FeePaid = request.FeePaid;

        if (!string.IsNullOrWhiteSpace(request.FeeDiscount))
            fee.FeeDiscount = request.FeeDiscount;

        if (request.FeeAmount > 0)
            fee.FeeAmount = request.FeeAmount;

        _context.Fees.Update(fee);
        await _context.SaveChangesAsync();

        return Ok(new { fee.FeeID, fee.FeePaid, fee.FeeDiscount, fee.FeeAmount, fee.PartID });
    }

    /// <summary>
    /// Deletes a fee record.
    /// </summary>
    /// <param name="id">Fee ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Fee deleted successfully</response>
    /// <response code="404">Fee not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteFee(int id)
    {
        var fee = await _context.Fees.FindAsync(id);
        if (fee == null)
            return NotFound(new { message = "Fee not found" });

        _context.Fees.Remove(fee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateFeeRequest
{
    public string? FeePaid { get; set; }
    public string? FeeDiscount { get; set; }
    public decimal FeeAmount { get; set; }
    public int PartID { get; set; }
}

public class UpdateFeeRequest
{
    public string? FeePaid { get; set; }
    public string? FeeDiscount { get; set; }
    public decimal FeeAmount { get; set; }
    public int PartID { get; set; }
}