using Microsoft.EntityFrameworkCore;
using FitnessAppBackend.Models;

namespace FitnessAppBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        //public DbSet<Workout> Workouts => Set<Workout>();
        //public DbSet<Exercise> Exercises => Set<Exercise>();
        //public DbSet<ExerciseInfo> ExerciseInfo => Set<ExerciseInfo>();
        //public DbSet<ExerciseLog> ExerciseLogs => Set<ExerciseLog>();
        //public DBSet<UserExercise> UserExercises => Set<UserExercise>();
        //public DbSet<Set> Sets => Set<Set>();
        //public DbSet<UserPersonalRecord> PersonalRecords => Set<UserPersonalRecord>();

        //public DBSet<Food> Foods => Set<Food>();
        //public DbSet<FoodLog> FoodLogs => Set<FoodLog>();
        //public DbSet<Macronutrients> Macronutrients => Set<Macronutrients>();

        //public DBSet<UserGoal> Goals => Set<UserGoal>();
        //public DBSet<ExerciseGoal> ExerciseGoals => Set<ExerciseGoal>();
        //public DbSet<WeightGoal> WeightGoals => Set<WeightGoal>();
        //public DbSet<NutritionGoals> NutritionGoals => Set<NutritionGoal>();
    }
}