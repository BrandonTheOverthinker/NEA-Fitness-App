namespace NEAFitnessApp.Models
{
    public record RegisterRequest
    {
        public int UserID { get; set; } 
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateOnly UserDOB { get; set; }
        public decimal BodyWeight { get; set; }
        public decimal Height { get; set; }
        public string Gender { get; set; } = "Prefer not to say";
    }
}
// completed
