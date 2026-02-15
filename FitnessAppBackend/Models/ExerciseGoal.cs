using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class ExerciseGoal
    {

        [Key]
        public int EGoalID { get; set; }

        public int GoalID { get; set; }
        [ForeignKey(nameof(GoalID))]
        public UserGoal? UserGoal { get; set; }

        public int ExerciseID { get; set; }
        [ForeignKey(nameof(ExerciseID))]
        public Exercise? Exercise { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal TargetWeight { get; set; }

        public int TargetTime { get; set; }
    }
}