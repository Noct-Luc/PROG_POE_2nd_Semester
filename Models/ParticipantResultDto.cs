namespace RaceDay.MVC.Models;

/// <summary>
/// Data transfer object for participant results.
/// </summary>
public class ParticipantResultDto
{
    public int ResultID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
    public int TotalFinishers { get; set; }
}