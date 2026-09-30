using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [StringLength(50)]
        public string EventName { get; set; }

        [Required]
        [StringLength(20)]
        public string EventClass { get; set; }

        [ForeignKey("Track")]
        public int? TrackID { get; set; }

        public virtual Track Track { get; set; }
        public virtual ICollection<TeamEvent> TeamEvents { get; set; }
    }
}