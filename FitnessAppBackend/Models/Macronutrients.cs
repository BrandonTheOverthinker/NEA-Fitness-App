using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class Macronutrients
    {
        [Key]
        public int MacroID { get; set; }

        public int FoodID { get; set; }
        [ForeignKey(nameof(FoodID))]
        public Food? Food { get; set; }

        public int Calories { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Protein { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Fat { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal SaturatedFat { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Carbohydrates { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Sugar { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Fibre { get; set; }
    }
}