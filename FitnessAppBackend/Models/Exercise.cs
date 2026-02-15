using System.ComponentModel.DataAnnotations;

namespace FitnessAppBackend.Models
{
    public class Exercise
    {
        [Key]
        public int ExerciseID { get; set; }

        [Required, MaxLength(50)]
        public string ExerciseName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string ExerciseType { get; set; } = string.Empty;
    }
}