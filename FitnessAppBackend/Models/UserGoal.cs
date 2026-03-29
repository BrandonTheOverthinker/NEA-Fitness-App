using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class UserGoal
    {
        [Key]
        public int GoalID { get; set; }

        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        [Required, MaxLength(25)]
        public string GoalType { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string GoalDescription { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }

        public DateTime Deadline { get; set; }

        public bool IsCompleted { get; set; }
    }
}