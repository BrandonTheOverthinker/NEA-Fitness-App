using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class Workout
    {
        [Key]
        public int WorkoutID { get; set; }

        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        public DateTime WorkoutTime { get; set; }

        public int WorkoutDurationS { get; set; }

        [Required, MaxLength(100)]
        public string WorkoutName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string WorkoutNotes { get; set; } = string.Empty;
    }
}
