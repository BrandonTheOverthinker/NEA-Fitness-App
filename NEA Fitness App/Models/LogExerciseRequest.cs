using System.ComponentModel.DataAnnotations;

namespace NEAFitnessApp.Models
{
    public record LogExerciseRequest
    {
        int WorkoutID { get; set; }
        int UserID { get; set; }
        int ExerciseID { get; set; }
        string ExerciseNotes { get; set; } = string.Empty;
    }
}
