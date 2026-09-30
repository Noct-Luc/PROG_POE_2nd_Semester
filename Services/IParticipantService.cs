using RaceDay.Models;

namespace RaceDay.Services
{
    public interface IParticipantService
    {
        Task<IEnumerable<Participant>> GetAllParticipantsAsync();
        Task<Participant> GetParticipantByIdAsync(int participantId);
        Task<Participant> CreateParticipantAsync(string partName, string partCar, int partWins, int? userId = null);
        Task<Participant> UpdateParticipantAsync(int participantId, string partName, string partCar, int partWins);
        Task<bool> DeleteParticipantAsync(int participantId);
    }
}