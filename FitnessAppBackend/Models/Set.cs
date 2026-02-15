using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class Set
    {
        [Key]
        public int SetID { get; set; }

        public int ExerciseLogID { get; set; }
        [ForeignKey(nameof(ExerciseLogID))]
        public ExerciseLog? ExerciseLog { get; set; }

        [Required, MaxLength(50)]
        public string SetType { get; set; } = string.Empty;

        public int SetNumber { get; set; }

        public int Reps { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal SetWeightKG { get; set; }

        public int DistanceM { get; set; }

        public int TimeSeconds { get; set; }

        [Required]
        [Column(TypeName = "decimal(3,1)")]
        public decimal RPE { get; set; }
    }
}