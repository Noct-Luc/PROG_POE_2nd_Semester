using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class Track
    {
        [Key]
        public int TrackID { get; set; }

        [Required]
        [StringLength(30)]
        public string TrackName { get; set; }

        [Required]
        [StringLength(30)]
        public string TrackLocation { get; set; }

        [StringLength(10)]
        public string TrackGrade { get; set; }

        public virtual ICollection<Event> Events { get; set; }
    }
}