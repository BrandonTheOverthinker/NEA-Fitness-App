using System.ComponentModel.DataAnnotations;

namespace FitnessAppBackend.Models
{
    public abstract class Exercise
    {
        [Key]
        public int ExerciseID { get; set; }

        [Required, MaxLength(50)]
        public string ExerciseName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string ExerciseType { get; set; } = string.Empty;

        public abstract string GetSetType();
    }

    public class StrengthExercise : Exercise
    {
        public StrengthExercise() => ExerciseType = "Strength";
        public override string GetSetType() => ExerciseType;
    }

    public class CardioExercise : Exercise
    {
        public CardioExercise() => ExerciseType = "Cardio";
        public override string GetSetType() => ExerciseType;
    }
}