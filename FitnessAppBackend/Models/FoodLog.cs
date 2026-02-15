using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class FoodLog
    {
        [Key]
        public int FoodLogID { get; set; }

        public int FoodID { get; set; }
        [ForeignKey(nameof(FoodID))]
        public Food? Food { get; set; }

        public DateTime LogTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Quantity { get; set; }
    }
}