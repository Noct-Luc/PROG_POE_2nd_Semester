using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Fee
    {
        [Key]
        public int FeeID { get; set; }

        [Required]
        [StringLength(10)]
        public string FeePaid { get; set; }

        [Required]
        [StringLength(20)]
        public string FeeDiscount { get; set; }

        [Required]
        [StringLength(20)]
        public decimal FeeAmount { get; set; }

        [ForeignKey("Participant")]
        public int PartID { get; set; }

        public virtual Participant Participant { get; set; }
    }
}