using System.ComponentModel.DataAnnotations;

namespace FitnessAppBackend.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
    }

    public class Exercise
    {

    }
    public class UserExercise
    {

    }
    public class Workout
    {

    }
    public class Set
    {

    }
    public class ExerciseLog
    {

    }
    public class UserPersonalRecord
    {

    }

    public class Food
    {

    }
    public class FoodLog
    {

    }
    public class Macronutrients
    {

    }

    public class UserGoal
    {

    }
    public class ExerciseGoal
    {

    }
    public class WeightGoal
    {

    }
    public class NutritionGoal
    {

    }

    public class XPLevel
    {

    }
}