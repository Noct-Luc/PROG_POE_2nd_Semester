namespace RaceDay.MVC.Models;

/// <summary>
/// View model for participant dashboard displaying enrollment statistics and list.
/// </summary>
public class ParticipantDashboardViewModel
{
    /// <summary>
    /// Gets or sets the total number of enrollments.
    /// </summary>
    public int TotalEnrollments { get; set; }

    /// <summary>
    /// Gets or sets the count of completed events.
    /// </summary>
    public int CompletedEvents { get; set; }

    /// <summary>
    /// Gets or sets the list of enrollment display information.
    /// </summary>
    public List<EnrollmentDisplayDto> Enrollments { get; set; } = new();
}

/// <summary>
/// DTO for displaying enrollment information in the dashboard.
/// </summary>
public class EnrollmentDisplayDto
{
    /// <summary>
    /// Gets or sets the enrollment ID.
    /// </summary>
    public int EnrolmentID { get; set; }

    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public int EventID { get; set; }

    /// <summary>
    /// Gets or sets the event title.
    /// </summary>
    public string EventTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event date.
    /// </summary>
    public DateTime? EventDate { get; set; }

    /// <summary>
    /// Gets or sets the participant category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the enrollment status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}