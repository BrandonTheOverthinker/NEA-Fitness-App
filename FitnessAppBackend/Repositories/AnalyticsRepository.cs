using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AppDbContext context;
        public AnalyticsRepository(AppDbContext context) => this.context = context;

        // Get total calories consumed today
        public async Task<decimal> GetTodayCaloriesAsync(int userId)
        {
            var logs = await context.FoodLogs
                .Include(fl => fl.FoodItem)
                .Where(fl => fl.UserID == userId && fl.LogTime.Date == DateTime.UtcNow.Date)
                .ToListAsync();

            return logs.Sum(log => log.FoodItem.Calories * log.Quantity);
        }

        // Get total calories for the week
        public async Task<decimal> GetWeeklyCaloriesAsync(int userId)
        {
            DateTime startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var logs = await context.FoodLogs
                .Include(fl => fl.FoodItem)
                .Where(fl => fl.UserID == userId && fl.LogTime >= startOfWeek)
                .ToListAsync();

            return logs.Sum(log => log.FoodItem.Calories * log.Quantity);
        }

        // Get 7-day rolling average for a specific macronutrient
        public async Task<decimal> Get7DayMacroAverageAsync(int userId, string macroType)
        {
            DateTime startDate = DateTime.UtcNow.Date.AddDays(-7);
            var logs = await context.FoodLogs
                .Include(fl => fl.FoodItem)
                .Where(fl => fl.UserID == userId && fl.LogTime >= startDate)
                .ToListAsync();

            decimal total = macroType switch
            {
                "Protein" => logs.Sum(log => log.FoodItem.Protein * log.Quantity),
                "Fat" => logs.Sum(log => log.FoodItem.Fat * log.Quantity),
                "Carbs" => logs.Sum(log => log.FoodItem.Carbohydrates * log.Quantity),
                "Calories" => logs.Sum(log => log.FoodItem.Calories * log.Quantity),
                "Fibre" => logs.Sum(log => log.FoodItem.Fibre * log.Quantity),
                _ => 0
            };

            return total / 7;
        }

        // Get macronutrient totals for today
        public async Task<MacronutrientTotals> GetTodayMacroTotalsAsync(int userId)
        {
            var logs = await context.FoodLogs
                .Include(fl => fl.FoodItem)
                .Where(fl => fl.UserID == userId && fl.LogTime.Date == DateTime.UtcNow.Date)
                .ToListAsync();

            return new MacronutrientTotals
            {
                Calories = logs.Sum(log => log.FoodItem.Calories * log.Quantity),
                Protein = logs.Sum(log => log.FoodItem.Protein * log.Quantity),
                Fat = logs.Sum(log => log.FoodItem.Fat * log.Quantity),
                Carbohydrates = logs.Sum(log => log.FoodItem.Carbohydrates * log.Quantity),
                Fibre = logs.Sum(log => log.FoodItem.Fibre * log.Quantity)
            };
        }

        // Get highest calorie food today
        public async Task<FoodItem?> GetHighestCalorieFoodTodayAsync(int userId)
        {
            var log = await context.FoodLogs
                .Include(fl => fl.FoodItem)
                .Where(fl => fl.UserID == userId && fl.LogTime.Date == DateTime.UtcNow.Date)
                .OrderByDescending(fl => fl.FoodItem.Calories)
                .FirstOrDefaultAsync();

            return log?.FoodItem;
        }

        // Get most recent food logged
        public async Task<FoodItem?> GetMostRecentFoodAsync(int userId)
        {
            var log = await context.FoodLogs
                .Include(fl => fl.FoodItem)
                .Where(fl => fl.UserID == userId)
                .OrderByDescending(fl => fl.LogTime)
                .FirstOrDefaultAsync();

            return log?.FoodItem;
        }

        // Get workouts this week
        public async Task<int> GetWeeklyWorkoutCountAsync(int userId)
        {
            DateTime startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            return await context.Workouts
                .Where(w => w.UserID == userId && w.WorkoutTime >= startOfWeek)
                .CountAsync();
        }

        // Get total workout duration this week (in minutes)
        public async Task<int> GetWeeklyWorkoutDurationAsync(int userId)
        {
            DateTime startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var workouts = await context.Workouts
                .Where(w => w.UserID == userId && w.WorkoutTime >= startOfWeek)
                .ToListAsync();

            return workouts.Sum(w => w.WorkoutDurationS) / 60;
        }

        // Get user's goals progress
        public async Task<GoalProgressSummary> GetGoalProgressAsync(int userId)
        {
            var goals = await context.Goals
                .Include(g => g.User)
                .Where(g => g.UserID == userId && !g.IsCompleted)
                .ToListAsync();

            var weightGoals = goals.Where(g => g.GoalType == "Weight Loss" || g.GoalType == "Weight Gain").ToList();
            var exerciseGoals = goals.Where(g => g.GoalType == "Exercise").ToList();
            var nutritionGoals = goals.Where(g => g.GoalType == "Nutrition").ToList();

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

            decimal weightProgress = 0;
            if (weightGoals.Any() && user != null)
            {
                var wg = await context.WeightGoals.FirstOrDefaultAsync(wg => wg.GoalID == weightGoals.First().GoalID);
                if (wg != null)
                {
                    decimal totalDiff = Math.Abs(wg.TargetBW - wg.StartBW);
                    decimal currentDiff = Math.Abs(wg.TargetBW - user.BodyWeight);
                    weightProgress = totalDiff > 0 ? (1 - (currentDiff / totalDiff)) * 100 : 0;
                    weightProgress = Math.Max(0, Math.Min(100, weightProgress));
                }
            }

            return new GoalProgressSummary
            {
                WeightGoalCount = weightGoals.Count,
                WeightGoalProgress = weightProgress,
                ExerciseGoalCount = exerciseGoals.Count,
                NutritionGoalCount = nutritionGoals.Count
            };
        }
    }

    // DTOs for analytics:
    public record MacronutrientTotals
    {
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Fat { get; set; }
        public decimal Carbohydrates { get; set; }
        public decimal Fibre { get; set; }
    }

    public record GoalProgressSummary
    {
        public int WeightGoalCount { get; set; }
        public decimal WeightGoalProgress { get; set; }
        public int ExerciseGoalCount { get; set; }
        public int NutritionGoalCount { get; set; }
    }
}
