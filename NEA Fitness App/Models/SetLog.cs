namespace NEAFitnessApp.Models
{   
    // I deserialise this from GET /sets/{exerciseLogId} to read the sets for a given exercise log:
    public record SetLog
    {
        public int SetID { get; set; }
        public int SetNumber { get; set; }
        public string SetType { get; set; } = string.Empty;
        public int Reps { get; set; }
        public decimal SetWeightKG { get; set; }
        public int DistanceM { get; set; }
        public int TimeSeconds { get; set; }
    }
}
