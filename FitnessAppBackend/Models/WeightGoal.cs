using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace FitnessAppBackend.Models
{
    public class WeightGoal
    {
        [Key]
        public int WGoalID { get; set; }

        public int GoalID { get; set; }
        [ForeignKey(nameof(GoalID))]
        public UserGoal? UserGoal { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal TargetBW { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal StartBW { get; set; }
    }
}