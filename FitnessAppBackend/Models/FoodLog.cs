using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class FoodLog
    {
        [Key]
        public int FoodLogID { get; set; }

        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        public int FoodItemID { get; set; }
        [ForeignKey(nameof(FoodItemID))]
        public FoodItem? FoodItem { get; set; }

        public DateTime LogTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,2)")]
        public decimal Quantity { get; set; }
    }
}