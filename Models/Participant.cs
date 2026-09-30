using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Participant
    {
        [Key]
        public int PartID { get; set; }

        [Required]
        [StringLength(20)]
        public string PartName { get; set; }

        [Required]
        [StringLength(30)]
        public string PartCar { get; set; }

        [Required]
        public int PartWins { get; set; }

        [ForeignKey("User")]
        public int? UserID { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Fee> Fees { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}