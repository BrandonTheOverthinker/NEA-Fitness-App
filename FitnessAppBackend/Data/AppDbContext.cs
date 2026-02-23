using Microsoft.EntityFrameworkCore;
using FitnessAppBackend.Models;

namespace FitnessAppBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<UserExercise> UserExercises => Set<UserExercise>();
        public DbSet<Workout> Workouts => Set<Workout>();
        public DbSet<Set> Sets => Set<Set>();
        public DbSet<ExerciseLog> ExerciseLogs => Set<ExerciseLog>();
        public DbSet<UserPersonalRecord> PersonalRecords => Set<UserPersonalRecord>();

        public DbSet<FoodItem> Foods => Set<FoodItem>();
        public DbSet<FoodLog> FoodLogs => Set<FoodLog>();
        public DbSet<Macronutrients> Macronutrients => Set<Macronutrients>();

        public DbSet<UserGoal> Goals => Set<UserGoal>();
        public DbSet<ExerciseGoal> ExerciseGoals => Set<ExerciseGoal>();
        public DbSet<WeightGoal> WeightGoals => Set<WeightGoal>();
        public DbSet<NutritionGoal> NutritionGoals => Set<NutritionGoal>();

        public DbSet<XPLevel> Level => Set<XPLevel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User"); // Clarifies that even if I call it 'Users' in C#, the SQL Table is actually 'User'

            modelBuilder.Entity<Exercise>().ToTable("Exercise");
            modelBuilder.Entity<UserExercise>().ToTable("UserExercise")
                .HasKey(ue => new { ue.UserID, ue.ExerciseID }); // Configure composite primary key for UserExercise
            modelBuilder.Entity<Workout>().ToTable("Workout");
            modelBuilder.Entity<Set>().ToTable("Set");
            modelBuilder.Entity<ExerciseLog>().ToTable("ExerciseLog");
            modelBuilder.Entity<UserPersonalRecord>().ToTable("UserPersonalRecord");

            modelBuilder.Entity<FoodItem>().ToTable("Food");
            modelBuilder.Entity<FoodLog>().ToTable("FoodLog");
            modelBuilder.Entity<Macronutrients>().ToTable("Macronutrients");

            modelBuilder.Entity<UserGoal>().ToTable("UserGoal")
                .Property(ug => ug.IsCompleted)
                .HasDefaultValue(false)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<ExerciseGoal>().ToTable("ExerciseGoal");
            modelBuilder.Entity<WeightGoal>().ToTable("WeightGoal");
            modelBuilder.Entity<NutritionGoal>().ToTable("NutritionGoal");

            modelBuilder.Entity<XPLevel>().ToTable("XPLevel");

            base.OnModelCreating(modelBuilder);
        }
    }
}