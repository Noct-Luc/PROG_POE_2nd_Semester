namespace RaceDay.MVC.Models;

/// <summary>
/// Data transfer object for enrolments.
/// </summary>
public class EnrolmentDto
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
}