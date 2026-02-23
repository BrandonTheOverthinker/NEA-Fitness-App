using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAFitnessApp.Models
{
    public class FoodLogEntry
    {
        public int FoodLogId { get; set; }
        public int UserId { get; set; }
        public int FoodItemId { get; set; }
        public DateTime LogTime { get; set; }
        public FoodItem FoodItem { get; set; }
    }
}
