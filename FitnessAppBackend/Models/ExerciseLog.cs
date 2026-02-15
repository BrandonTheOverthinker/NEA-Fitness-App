using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class ExerciseLog
    {
        [Key]
        public int ExerciseLogID { get; set; }

        public int WorkoutID { get; set; }
        [ForeignKey(nameof(WorkoutID))]
        public Workout? Workout { get; set; }

        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        public int ExerciseID { get; set; }
        [ForeignKey(nameof(ExerciseID))]
        public Exercise? Exercise { get; set; }

        public int ExerciseOrder { get; set; }

        [MaxLength(500)]
        public string ExerciseNotes { get; set; } = string.Empty;
    }
}