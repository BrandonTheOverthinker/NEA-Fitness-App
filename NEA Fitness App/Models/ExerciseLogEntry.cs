namespace NEAFitnessApp.Models
{
    public record ExerciseLogEntry
    {
        public int ExerciseLogID { get; set; }
        public int WorkoutID { get; set; }
        public int UserID { get; set; }
        public int ExerciseID { get; set; }
        public string ExerciseNotes { get; set; } = string.Empty; // Add feature to log notes about individual exercise.
                                                                  // (e.g. "Set seat height to 5" could be a note for Lat Pulldown).

        // Included in history responses so I can read the workout date for the chart:
        public WorkoutSummary? Workout { get; set; }
    }
}
