using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAFitnessApp.Models
{
    public record FoodLogEntry
    {
        public int FoodLogID { get; set; }
        public int UserID { get; set; }
        public int FoodItemID { get; set; }
        public DateTime LogTime { get; set; }
        public decimal Quantity { get; set; }
        public FoodItem FoodItem { get; set; }
    }
}
