using FitnessAppBackend.Models;
using FitnessAppBackend.Repositories;

namespace FitnessAppBackend.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<decimal> GetTodayCaloriesAsync(int userId);
        Task<decimal> GetWeeklyCaloriesAsync(int userId);
        Task<decimal> Get7DayMacroAverageAsync(int userId, string macroType);
        Task<MacronutrientTotals> GetTodayMacroTotalsAsync(int userId);
        Task<FoodItem?> GetHighestCalorieFoodTodayAsync(int userId);
        Task<FoodItem?> GetMostRecentFoodAsync(int userId);
        Task<int> GetWeeklyWorkoutCountAsync(int userId);
        Task<int> GetWeeklyWorkoutDurationAsync(int userId);
        Task<GoalProgressSummary> GetGoalProgressAsync(int userId);
    }
}
