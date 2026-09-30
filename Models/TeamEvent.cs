using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class TeamEvent
    {
        [Key]
        public int EventTeamID { get; set; }

        [ForeignKey("Event")]
        public int? EventID { get; set; }

        [ForeignKey("Team")]
        public int? TeamID { get; set; }

        public virtual Event Event { get; set; }
        public virtual Team Team { get; set; }
    }
}