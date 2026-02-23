using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessAppBackend.Models
{
    public class FoodItem
    {
        [Key]
        public int FoodItemID { get; set; }

        [Required, MaxLength(100)]
        public string FoodName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Barcode { get; set; } = string.Empty; // ? means nullable

        public int Calories { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Protein { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Fat { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal SaturatedFat { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Carbohydrates { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Sugar { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        public decimal Fibre { get; set; }

        public int? CreatedByUserID { get; set; }
        [ForeignKey(nameof(CreatedByUserID))]
        public User? User { get; set; }
    }
}