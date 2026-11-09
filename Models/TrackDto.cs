namespace RaceDay.MVC.Models;

/// <summary>
/// Data transfer object for tracks.
/// </summary>
public class TrackDto
{
    public int TrackID { get; set; }
    public string TrackName { get; set; } = string.Empty;
    public string TrackLocation { get; set; } = string.Empty;
}