namespace NEAFitnessApp.Models
{
    // I build this in ExerciseHistoryPage from the raw API data - it is not a direct API response.
    // The CollectionView in ExerciseHistoryPage.xaml binds to these properties:
    public record ExerciseHistoryEntry
    {
        public string DateStr { get; set; } = string.Empty; // e.g. "13 Feb"
        public string SetCount { get; set; } = string.Empty; // e.g. "3 sets"
        public string BestSet { get; set; } = string.Empty; // e.g. "50kg x 8" or "5km in 30:00"
        public string TotalVolume { get; set; } = string.Empty; // e.g. "1000" (kg) or "10:00" (time)
        public int ExerciseLogID { get; set; } // I also store the raw ExerciseLogID so I can fetch the sets for this session
    }
}