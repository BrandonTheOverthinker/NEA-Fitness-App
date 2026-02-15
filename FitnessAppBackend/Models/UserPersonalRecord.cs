using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class UserPersonalRecord
    {
        [Key]
        public int PRID { get; set; }

        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public User? User { get; set; }

        public int ExerciseID { get; set; }
        [ForeignKey(nameof(ExerciseID))]
        public Exercise? Exercise { get; set; }

        [MaxLength(20)]
        public string PRType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PRValue { get; set; }

        public DateTime AchievedAt { get; set; }

        public int SetID { get; set; }
        [ForeignKey(nameof(SetID))]
        public Set? Set { get; set; }
    }
}