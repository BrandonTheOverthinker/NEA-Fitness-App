using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAFitnessApp.Models
{
    public record Exercise
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string ExerciseType { get; set; } = string.Empty; // "Strength" or "Cardio"
    }
}
