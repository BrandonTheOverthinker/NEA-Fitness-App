namespace NEAFitnessApp.Models
{
    public record UserGoalResponse
    {
        public int GoalID { get; set; }
        public int UserID { get; set; }
        public string GoalType { get; set; } = string.Empty;
        public string GoalDescription { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public bool IsCompleted { get; set; }
        public int DaysUntilDeadline { get; set; }
        public WeightGoalData? WeightGoalData { get; set; }
        public ExerciseGoalData? ExerciseGoalData { get; set; }
    }

    public record WeightGoalData
    {
        public int WGoalID { get; set; }
        public decimal TargetBW { get; set; }
        public decimal StartBW { get; set; }
    }

    public record ExerciseGoalData
    {
        public int EGoalID { get; set; }
        public int ExerciseID { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string ExerciseType { get; set; } = string.Empty;
        public decimal TargetWeight { get; set; }
        public int TargetDistance { get; set; }
        public int TargetTime { get; set; }
    }
}