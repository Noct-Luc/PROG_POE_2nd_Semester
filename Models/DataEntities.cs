namespace RaceDay.Models;

/// <summary>
/// Represents a user account in the system.
/// </summary>
public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string UserRole { get; set; } = "Participant"; // "Manager" or "Participant"
    
    // Navigation property
    public ICollection<Participant>? Participants { get; set; }
}

/// <summary>
/// Represents a race event participant.
/// </summary>
public class Participant
{
    public int PartID { get; set; }
    public string Partname { get; set; } = string.Empty;
    public string PartCar { get; set; } = string.Empty;
    public int PartWins { get; set; }
    public int UserID { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Fee>? Fees { get; set; }
    public ICollection<Team>? Teams { get; set; }
}

/// <summary>
/// Represents a fee payment record for a participant.
/// </summary>
public class Fee
{
    public int FeeID { get; set; }
    public string FeePaid { get; set; } = "No";
    public string FeeDiscount { get; set; } = "No";
    public decimal FeeAmount { get; set; }
    public int PartID { get; set; }

    // Navigation property
    public Participant? Participant { get; set; }
}

/// <summary>
/// Represents a racing team.
/// </summary>
public class Team
{
    public int TeamID { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string TeamPosition { get; set; } = string.Empty;
    public int PartID { get; set; }

    // Navigation properties
    public Participant? Participant { get; set; }
    public ICollection<TeamEvent>? TeamEvents { get; set; }
}

/// <summary>
/// Represents a race track.
/// </summary>
public class Track
{
    public int TrackID { get; set; }
    public string TrackName { get; set; } = string.Empty;
    public string TrackLocation { get; set; } = string.Empty;
    public string TrackGrade { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Event>? Events { get; set; }
}

/// <summary>
/// Represents a racing event.
/// </summary>
public class Event
{
    public int EventID { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string EventClass { get; set; } = string.Empty;
    public int TrackID { get; set; }

    // Navigation properties
    public Track? Track { get; set; }
    public ICollection<TeamEvent>? TeamEvents { get; set; }
}

/// <summary>
/// Represents the relationship between a team and an event (team enrollment).
/// </summary>
public class TeamEvent
{
    public int EventTeamID { get; set; }
    public int TeamID { get; set; }
    public int EventID { get; set; }

    // Navigation properties
    public Team? Team { get; set; }
    public Event? Event { get; set; }
}