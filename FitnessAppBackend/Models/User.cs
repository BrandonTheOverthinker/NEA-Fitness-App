using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public DateOnly UserDOB { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal BodyWeight { get; set; } // KG

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal Height { get; set; } // CM

        [Required, MaxLength(20)]
        public string Gender { get; set; } = "Prefer not to say";

        [Required, MaxLength(20)]
        public string ActivityLevel { get; set; } = "Sedentary";

        [Required]
        [Column(TypeName = "decimal(5,0)")]
        public decimal MaintenanceGoal { get; set; }
    }
}