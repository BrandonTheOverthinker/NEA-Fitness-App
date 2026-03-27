using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAFitnessApp.Models
{
    public record FoodItem
    {
        public int FoodItemId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public string? Barcode { get; set; }

        public int Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Fat { get; set; }
        public decimal SaturatedFat { get; set; }
        public decimal Carbohydrates { get; set; }
        public decimal Sugar { get; set; }
        public decimal Fibre { get; set; }
        public decimal Quantity { get; set; }

        public int? CreatedByUserID { get; set; }
    }
}
