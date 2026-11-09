namespace RaceDay.MVC.Models;

/// <summary>
/// View model for entering results.
/// </summary>
public class ResultsEntryViewModel
{
    public int EventID { get; set; }
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}