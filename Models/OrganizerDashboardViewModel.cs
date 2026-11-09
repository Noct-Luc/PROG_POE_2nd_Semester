
namespace RaceDay.MVC.Models;

public class OrganizerDashboardViewModel
{
    public int TotalEvents { get; set; }

    public int UpcomingEventsCount { get; set; }

    public List<EventDto> Events { get; set; } = new();
}
