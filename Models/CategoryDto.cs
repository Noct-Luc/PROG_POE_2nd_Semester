namespace RaceDay.MVC.Models;

/// <summary>
/// Data transfer object for categories.
/// </summary>
public class CategoryDto
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
}