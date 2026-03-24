namespace NEAFitnessApp.Models
{
    public record CreateExerciseRequest
    {
        int UserID { get; set; }
        string ExerciseName { get; set; } = string.Empty;
        string ExerciseType { get; set; } = string.Empty;

    }
}
