namespace NEAFitnessApp.Models
{
    // I deserialise this from the POST log-exercise response:
    public record ExerciseLogEntry
    {
        public int ExerciseLogID { get; set; }
        public int WorkoutID { get; set; }
        public int UserID { get; set; }
        public int ExerciseID { get; set; }
        public string ExerciseNotes { get; set; } = string.Empty; // Add feature to log notes about individual exercise.
                                                                  // (e.g. "Set seat height to 5" on Lat Pulldowns).
    }
}
