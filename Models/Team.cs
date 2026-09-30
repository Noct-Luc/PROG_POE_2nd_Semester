using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Team
    {
        [Key]
        public int TeamID { get; set; }

        [Required]
        [StringLength(20)]
        public string TeamName { get; set; }

        [StringLength(20)]
        public string TeamPosition { get; set; }

        [ForeignKey("Participant")]
        public int PartID { get; set; }

        public virtual Participant Participant { get; set; }
        public virtual ICollection<TeamEvent> TeamEvents { get; set; }
    }
}