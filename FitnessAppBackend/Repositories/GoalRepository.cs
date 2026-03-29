using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class GoalRepository : IGoalRepository
    {
        private readonly AppDbContext context;
        public GoalRepository(AppDbContext context) => this.context = context;

        // Enforce only one active weight goal per user:
        public async Task<UserGoal> CreateWeightGoalAsync(int userId, string description, DateTime deadline, decimal targetWeight, decimal startWeight)
        {
            // Mark any existing active weight goal as completed (keep history):
            var existing = await context.Goals
                .Where(g => g.UserID == userId && (g.GoalType == "Weight Loss" || g.GoalType == "Weight Gain") && !g.IsCompleted)
                .ToListAsync();

            if (existing.Any())
            {
                foreach (var g in existing)
                    g.IsCompleted = true;
                await context.SaveChangesAsync();
            }

            var userGoal = new UserGoal
            {
                UserID = userId,
                GoalType = targetWeight < startWeight ? "Weight Loss" : "Weight Gain",
                GoalDescription = description,
                DateCreated = DateTime.UtcNow,
                Deadline = deadline,
                IsCompleted = false
            };

            context.Goals.Add(userGoal);
            await context.SaveChangesAsync();

            var weightGoal = new WeightGoal
            {
                GoalID = userGoal.GoalID,
                TargetBW = targetWeight,
                StartBW = startWeight
            };

            context.WeightGoals.Add(weightGoal);
            await context.SaveChangesAsync();

            return userGoal;
        }

        public async Task<UserGoal> CreateExerciseGoalAsync(int userId, string description, DateTime deadline, int exerciseId, decimal targetWeight, int targetTime)
        {
            var userGoal = new UserGoal
            {
                UserID = userId,
                GoalType = "Exercise",
                GoalDescription = description,
                DateCreated = DateTime.UtcNow,
                Deadline = deadline,
                IsCompleted = false
            };

            context.Goals.Add(userGoal);
            await context.SaveChangesAsync();

            var exerciseGoal = new ExerciseGoal
            {
                GoalID = userGoal.GoalID,
                ExerciseID = exerciseId,
                TargetWeight = targetWeight,
                TargetTime = targetTime
            };

            context.ExerciseGoals.Add(exerciseGoal);
            await context.SaveChangesAsync();

            return userGoal;
        }

        public async Task<List<UserGoal>> GetUserGoalsAsync(int userId) =>
            await context.Goals
                .Where(g => g.UserID == userId)
                .OrderByDescending(g => g.DateCreated)
                .ToListAsync();

        public async Task<UserGoal?> GetGoalByIdAsync(int goalId) =>
            await context.Goals
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.GoalID == goalId);

        public async Task CompleteGoalAsync(int goalId)
        {
            var goal = await context.Goals.FindAsync(goalId);
            if (goal != null)
            {
                goal.IsCompleted = true;
                await context.SaveChangesAsync();
            }
        }

        // Delete goal and related child rows (weight/exercise/nutrition):
        public async Task DeleteGoalAsync(int goalId)
        {
            var weight = await context.WeightGoals.FirstOrDefaultAsync(w => w.GoalID == goalId);
            if (weight != null)
            {
                // Delete related nutrition if exists:
                var nutrition = await context.NutritionGoals.FirstOrDefaultAsync(n => n.WGoalID == weight.WGoalID);
                if (nutrition != null)
                    context.NutritionGoals.Remove(nutrition);

                context.WeightGoals.Remove(weight);
            }

            var exercise = await context.ExerciseGoals.FirstOrDefaultAsync(e => e.GoalID == goalId);
            if (exercise != null)
                context.ExerciseGoals.Remove(exercise);

            var userGoal = await context.Goals.FindAsync(goalId);
            if (userGoal != null)
                context.Goals.Remove(userGoal);

            await context.SaveChangesAsync();
        }

        public async Task<NutritionGoal> CreateNutritionGoalAsync(int weightGoalId, int calorieGoal, decimal proteinGoal, decimal fatGoal, decimal satFatGoal, decimal carbsGoal, decimal sugarGoal, decimal fibreGoal)
        {
            var nutritionGoal = new NutritionGoal
            {
                WGoalID = weightGoalId,
                CalorieGoal = calorieGoal,
                ProteinGoal = proteinGoal,
                FatGoal = fatGoal,
                SaturatedFatGoal = satFatGoal,
                CarbohydratesGoal = carbsGoal,
                SugarGoal = sugarGoal,
                FibreGoal = fibreGoal
            };

            context.NutritionGoals.Add(nutritionGoal);
            await context.SaveChangesAsync();

            return nutritionGoal;
        }

        public async Task<WeightGoal?> GetWeightGoalByGoalIdAsync(int goalId) =>
            await context.WeightGoals
                .FirstOrDefaultAsync(wg => wg.GoalID == goalId);

        public async Task<ExerciseGoal?> GetExerciseGoalByGoalIdAsync(int goalId) =>
            await context.ExerciseGoals
                .Include(eg => eg.Exercise)
                .FirstOrDefaultAsync(eg => eg.GoalID == goalId);
    }
}
