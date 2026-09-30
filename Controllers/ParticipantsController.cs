using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Services;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantsController : ControllerBase
    {
        private readonly IParticipantService _participantService;

        public ParticipantsController(IParticipantService participantService)
        {
            _participantService = participantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllParticipants()
        {
            var participants = await _participantService.GetAllParticipantsAsync();
            return Ok(participants);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParticipantById(int id)
        {
            var participant = await _participantService.GetParticipantByIdAsync(id);
            if (participant == null)
                return NotFound();

            return Ok(participant);
        }

        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateParticipant([FromBody] dynamic request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                var participant = await _participantService.CreateParticipantAsync(
                    request.PartName, request.PartCar, request.PartWins, userId);

                return CreatedAtAction(nameof(GetParticipantById), new { id = participant.PartID }, participant);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateParticipant(int id, [FromBody] dynamic request)
        {
            try
            {
                var participant = await _participantService.UpdateParticipantAsync(id, request.PartName, request.PartCar, request.PartWins);
                if (participant == null)
                    return NotFound();

                return Ok(participant);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var result = await _participantService.DeleteParticipantAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}