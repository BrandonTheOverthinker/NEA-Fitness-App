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
        public decimal BodyWeight { get; set; }

        [MaxLength(5)]
        public decimal Height { get; set; }
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
        public string PRType { get; set; }

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
        public string FoodName { get; set; }
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