using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AppDbContext context;
        public AnalyticsRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<decimal> GetTodayCalories(int userId)
        {
            // Fetch logged food by matching UserID with today's date. Then select calories and sum them up by multiplying food calories and quantity for each log:
            var logs = await context.FoodLogs.Include(fl => fl.FoodItem) .Where(fl => fl.UserID == userId && fl.LogTime.Date == DateTime.UtcNow.Date).ToListAsync();
            return logs.Sum(log => log.FoodItem.Calories * log.Quantity);
        }

        public async Task<decimal> GetWeeklyCalories(int userId)
        {
            DateTime startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            // Same as above but with date filter for the whole week. Used to show the calorie trend in the analytics page:
            var logs = await context.FoodLogs.Include(fl => fl.FoodItem).Where(fl => fl.UserID == userId && fl.LogTime >= startOfWeek).ToListAsync();

            return logs.Sum(log => log.FoodItem.Calories * log.Quantity);
        }

        public async Task<decimal> Get7DayMacroAverage(int userId, string macroType)
        {
            DateTime startDate = DateTime.UtcNow.Date.AddDays(-7);
            // Same again but for last 7 days instead of first day of the week, and with a switch statement to select which macro to sum up.
            // Used to show the macro trends in the analytics page:
            var logs = await context.FoodLogs.Include(fl => fl.FoodItem).Where(fl => fl.UserID == userId && fl.LogTime >= startDate).ToListAsync();

            decimal total = 0;
            switch (macroType)
            {
                case "Protein":
                    foreach (var log in logs)
                        total += log.FoodItem.Protein * log.Quantity;
                    break;

                case "Fat":
                    foreach (var log in logs)
                        total += log.FoodItem.Fat * log.Quantity;
                    break;

                case "Carbs":
                    foreach (var log in logs)
                        total += log.FoodItem.Carbohydrates * log.Quantity;
                    break;

                case "Calories":
                    foreach (var log in logs)
                        total += log.FoodItem.Calories * log.Quantity;
                    break;

                case "Fibre":
                    foreach (var log in logs)
                        total += log.FoodItem.Fibre * log.Quantity;
                    break;

                default:
                    total = 0;
                    break;
            }
            return total / 7;
        }

        public async Task<MacronutrientTotals> GetTodayMacroTotals(int userId)
        {
            // Fetch current day's food logs and sum up each macro by multiplying the food's macro values with the quantity for each log.
            // Used to show macro breakdown for current date in the analytics page:
            var logs = await context.FoodLogs.Include(fl => fl.FoodItem).Where(fl => fl.UserID == userId && fl.LogTime.Date == DateTime.UtcNow.Date).ToListAsync();

            return new MacronutrientTotals
            {
                Calories = logs.Sum(log => log.FoodItem.Calories * log.Quantity),
                Protein = logs.Sum(log => log.FoodItem.Protein * log.Quantity),
                Fat = logs.Sum(log => log.FoodItem.Fat * log.Quantity),
                Carbohydrates = logs.Sum(log => log.FoodItem.Carbohydrates * log.Quantity),
                Fibre = logs.Sum(log => log.FoodItem.Fibre * log.Quantity)
            };
        }

        public async Task<FoodItem?> GetHighestCalorieFoodToday(int userId)
        {
            // Fetch food logs, order by descending calories and use first item in list since it must have the highest calories.
            var log = await context.FoodLogs.Include(fl => fl.FoodItem).Where(fl => fl.UserID == userId && fl.LogTime.Date == DateTime.UtcNow.Date)
                .OrderByDescending(fl => fl.FoodItem.Calories).FirstOrDefaultAsync();
            return log?.FoodItem;
        }

        public async Task<FoodItem?> GetMostRecentFood(int userId)
        {
            // Same as previous method but sort by date instead
            var log = await context.FoodLogs.Include(fl => fl.FoodItem).Where(fl => fl.UserID == userId)
                .OrderByDescending(fl => fl.LogTime).FirstOrDefaultAsync();
            return log?.FoodItem;
        }

        public async Task<int> GetWeeklyWorkoutCount(int userId)
        {
            // Fetch workouts for the current week and count them
            DateTime startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            return await context.Workouts.Where(w => w.UserID == userId && w.WorkoutTime >= startOfWeek).CountAsync();
        }

        public async Task<int> GetWeeklyWorkoutDuration(int userId)
        {
            DateTime startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var workouts = await context.Workouts.Where(w => w.UserID == userId && w.WorkoutTime >= startOfWeek).ToListAsync();

            return workouts.Sum(w => w.WorkoutDurationS) / 60; // convert sum from seconds to minutes
        }

        public async Task<GoalProgressSummary> GetGoalProgress(int userId)
        {
            // Fetch all active goals for the user and categorize them by type.
            // Then calculate progress for weight goals by comparing current body weight with target weight and start weight.
            var goals = await context.Goals.Include(g => g.User).Where(g => g.UserID == userId && !g.IsCompleted).ToListAsync();

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

    // Models used for analytics:
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
