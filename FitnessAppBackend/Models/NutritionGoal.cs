using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class NutritionGoal
    {
        [Key]
        public int NGoalID { get; set; }

        public int WGoalID { get; set; }
        [ForeignKey(nameof(WGoalID))]
        public WeightGoal? WeightGoal { get; set; }

        public int CalorieGoal { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal ProteinGoal { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal FatGoal { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal SaturatedFatGoal { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal CarbohydratesGoal { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal SugarGoal { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal FibreGoal { get; set; }
    }
}