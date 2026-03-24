namespace NEAFitnessApp.Models
{
    public record LogSetRequest
    {
        int ExerciseID { get; set; }
        int SetNumber { get; set; }
        string SetType { get; set; } = string.Empty;
        int Reps { get; set; }
        int SetWeightKG { get; set; }
        int DistanceM { get; set; }
        int TimeSeconds { get; set; }
    }
}
