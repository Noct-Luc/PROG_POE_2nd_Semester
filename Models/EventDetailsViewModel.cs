namespace RaceDay.MVC.Models;

/// <summary>
/// View model for event details.
/// </summary>
public class EventDetailsViewModel
{
    public EventDto? Event { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}