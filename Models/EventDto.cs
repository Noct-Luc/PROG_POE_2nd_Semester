namespace RaceDay.MVC.Models;

public class EventDto
{
    public int EventID { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public string EventClass { get; set; } = string.Empty;

    // Compatibility property for existing MVC code
    public string EventType
    {
        get => EventClass;
        set => EventClass = value;
    }

    public int TrackID { get; set; }
}