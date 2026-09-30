using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(20)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        public string UserRole { get; set; } // "Manager" or "Participant"
    }
}