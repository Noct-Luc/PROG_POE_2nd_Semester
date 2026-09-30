using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(20)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string UserRole { get; set; } // "Manager" or "Participant"

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}