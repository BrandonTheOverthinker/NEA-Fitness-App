namespace NEAFitnessApp.Models
{
    public record StartWorkoutRequest
    {
        int UserID { get; set; }
        string WorkoutName { get; set; } = string.Empty;
        string WorkoutNotes { get ; set; } = string.Empty; // Maybe move to finish workout request
    }
}
