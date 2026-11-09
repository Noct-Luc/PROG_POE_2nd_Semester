namespace RaceDay.MVC.Models;

/// <summary>
/// View model for creating an event.
/// </summary>
public class CreateEventViewModel
{
    public string EventTitle { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string EventLocation { get; set; } = string.Empty;
    public decimal EventDistance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int TrackID { get; set; }
    public IFormFile? BannerImage { get; set; }
    public List<TrackDto> Tracks { get; set; } = new();
}