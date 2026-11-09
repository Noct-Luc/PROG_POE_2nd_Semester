namespace RaceDay.MVC.Models;

/// <summary>
/// View model for viewing enrolments.
/// </summary>
public class EnrolmentsViewModel
{
    public EventDto? Event { get; set; }
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}