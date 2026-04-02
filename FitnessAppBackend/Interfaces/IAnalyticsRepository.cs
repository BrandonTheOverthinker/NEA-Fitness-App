using FitnessAppBackend.Models;
using FitnessAppBackend.Repositories;

namespace FitnessAppBackend.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<decimal> GetTodayCalories(int userId);
        Task<decimal> GetWeeklyCalories(int userId);
        Task<decimal> Get7DayMacroAverage(int userId, string macroType);
        Task<MacronutrientTotals> GetTodayMacroTotals(int userId);
        Task<FoodItem?> GetHighestCalorieFoodToday(int userId);
        Task<FoodItem?> GetMostRecentFood(int userId);
        Task<int> GetWeeklyWorkoutCount(int userId);
        Task<int> GetWeeklyWorkoutDuration(int userId);
        Task<GoalProgressSummary> GetGoalProgress(int userId);
    }
}
