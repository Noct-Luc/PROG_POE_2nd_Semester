using RaceDay.Data;
using RaceDay.Models;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Services
{
    public class ParticipantService : IParticipantService
    {
        private readonly RaceDayDbContext _context;

        public ParticipantService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Participant>> GetAllParticipantsAsync()
        {
            return await _context.Participants.Include(p => p.User).ToListAsync();
        }

        public async Task<Participant> GetParticipantByIdAsync(int participantId)
        {
            return await _context.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PartID == participantId);
        }

        public async Task<Participant> CreateParticipantAsync(string partName, string partCar, int partWins, int? userId = null)
        {
            var participant = new Participant
            {
                PartName = partName,
                PartCar = partCar,
                PartWins = partWins,
                UserID = userId
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<Participant> UpdateParticipantAsync(int participantId, string partName, string partCar, int partWins)
        {
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.PartID == participantId);
            if (participant == null)
                return null;

            participant.PartName = partName;
            participant.PartCar = partCar;
            participant.PartWins = partWins;

            _context.Participants.Update(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<bool> DeleteParticipantAsync(int participantId)
        {
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.PartID == participantId);
            if (participant == null)
                return false;

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}