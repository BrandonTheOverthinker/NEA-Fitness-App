namespace NEAFitnessApp.Models
{
    // I deserialise this from the POST /start response to get the generated WorkoutID:
    public record WorkoutSummary
    {
        public int WorkoutID { get; set; }
        public int UserID { get; set; }
        public DateTime WorkoutTime { get; set; }
        public int WorkoutDurationS { get; set; }
        public string WorkoutName { get; set; } = string.Empty;
        public string WorkoutNotes { get; set; } = string.Empty;
    }
}