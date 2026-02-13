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

        public DateOnly UserDOB { get; set; }

        [MaxLength(5)]
        public decimal BodyWeight { get; set; } // KG

        [MaxLength(5)]
        public decimal Height { get; set; } // CM
    }

    public class Exercise
    {
        public int ExerciseID { get; set; }

        [Required, MaxLength(50)]
        public string ExerciseName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string ExerciseType {  get; set; } = string.Empty;
    }
    public class UserExercise
    {
        public int UserID { get; set; }
        
        public int ExerciseID { get; set; }
    }
    public class Workout
    {
        public int WorkoutID { get; set; }

        public int UserID { get; set; }

        public DateTime WorkoutTime { get; set; }

        public int WorkoutDurationS { get; set; }

        [Required, MaxLength(100)]
        public string WorkoutName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string WorkoutNotes { get; set; } = string.Empty;
    }
    public class Set
    {
        public int SetID { get; set; }

        public int ExerciseLogID { get; set; }

        [Required, MaxLength(50)]
        public string SetType {  get; set; } = string.Empty;

        public int SetNumber { get; set; }

        public int Reps { get; set; }

        [MaxLength(7)]
        public decimal SetWeightKG { get; set; }

        public int DistanceM { get; set; }

        public int TimeSeconds { get; set; }

        [MaxLength(3)]
        public decimal RPE { get; set; }
    }
    public class ExerciseLog
    {
        public int ExerciseLogID { get; set; }

        public int WorkoutID { get; set; }

        public int UserID { get; set; }

        public int ExerciseID { get; set; }

        public int ExerciseOrder { get; set; }

        [MaxLength(500)]
        public string ExerciseNotes {  get; set; } = string.Empty;
    }
    public class UserPersonalRecord
    {
        public int PRID { get; set; }

        public int UserID { get; set; }

        public int ExerciseID { get; set; }

        [MaxLength(20)]
        public string PRType { get; set; } = string.Empty;

        [MaxLength(10)]
        public decimal PRValue { get; set; }

        public DateTime AchievedAt { get; set; }

        public int SetID { get; set; }
    }

    public class Food
    {
        public int FoodID { get; set; }

        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string FoodName { get; set; } = string.Empty;
    }
    public class FoodLog
    {
        public int FoodLogID { get; set; }

        public int FoodID { get; set; }

        public DateTime LogTime { get; set; }

        [Required, MaxLength(7)]
        public decimal Quantity { get; set; }
    }
    public class Macronutrients
    {
        public int MacroID { get; set; }

        public int FoodID { get; set; }

        public int Calories { get; set; }

        [Required, MaxLength(7)]
        public decimal Protein { get; set; }

        [Required, MaxLength(7)]
        public decimal Fat { get; set; }

        [Required, MaxLength(7)]
        public decimal SaturatedFat { get; set; }

        [Required, MaxLength(7)]
        public decimal Carbohydrates { get; set; }

        [Required, MaxLength(7)]
        public decimal Sugar { get; set; }

        [Required, MaxLength(7)]
        public decimal Fibre { get; set; }
    }

    public class UserGoal
    {
        public int GoalID { get; set; }

        public int UserID { get; set; }

        [Required, MaxLength(25)]
        public string GoalType { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string GoalDescription { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }

        public bool IsCompleted { get; set; }
    }
    public class ExerciseGoal
    {
        public int EGoalID { get; set; }
        
        public int GoalID { get; set; }

        public int ExerciseID { get; set; }

        [Required, MaxLength(7)]
        public decimal TargetWeight { get; set; }

        public int TargetTime { get; set; }
    }
    public class WeightGoal
    {
        public int WGoalID { get; set; }

        public int GoalID { get; set; }

        [Required, MaxLength (5)]
        public decimal TargetBW { get; set; }

        [Required, MaxLength(5)]
        public decimal StartBW { get; set; }
    }
    public class NutritionGoal
    {
        public int NGoalID { get; set; }

        public int WGoalID { get; set; }

        public int CalorieGoal { get; set; }

        [Required, MaxLength(7)]
        public decimal ProteinGoal { get; set; }

        [Required, MaxLength(7)]
        public decimal FatGoal { get; set; }

        [Required, MaxLength(7)]
        public decimal SaturatedFatGoal { get; set; }

        [Required, MaxLength(7)]
        public decimal CarbohydratesGoal { get; set; }

        [Required, MaxLength(7)]
        public decimal SugarGoal { get; set; }

        [Required, MaxLength(7)]
        public decimal FibreGoal { get; set; }
    }

    public class XPLevel
    {
        public int TotalXP { get; set; }

        public int UserID { get; set; }

        public int Level { get; set; }
    }
}