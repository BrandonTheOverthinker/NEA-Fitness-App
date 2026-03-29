using FitnessAppBackend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsRepository analyticsRepo;

        public AnalyticsController(IAnalyticsRepository analyticsRepo) => this.analyticsRepo = analyticsRepo;

        // GET api/analytics/user/{userId}/summary
        [HttpGet("user/{userId}/summary")]
        public async Task<IActionResult> GetAnalyticsSummary(int userId)
        {
            try
            {
                var todayCalories = await analyticsRepo.GetTodayCaloriesAsync(userId);
                var weeklyCalories = await analyticsRepo.GetWeeklyCaloriesAsync(userId);
                var weeklyWorkouts = await analyticsRepo.GetWeeklyWorkoutCountAsync(userId);
                var weeklyDuration = await analyticsRepo.GetWeeklyWorkoutDurationAsync(userId);
                var highestFood = await analyticsRepo.GetHighestCalorieFoodTodayAsync(userId);
                var recentFood = await analyticsRepo.GetMostRecentFoodAsync(userId);
                var goalProgress = await analyticsRepo.GetGoalProgressAsync(userId);

                return Ok(new
                {
                    todayCalories = Math.Round(todayCalories, 0),
                    weeklyCalories = Math.Round(weeklyCalories, 0),
                    weeklyWorkouts,
                    weeklyDuration,
                    highestFood = new { foodName = highestFood?.FoodName, calories = highestFood?.Calories },
                    recentFood = new { foodName = recentFood?.FoodName, calories = recentFood?.Calories },
                    goalProgress
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving analytics: {ex.Message}");
            }
        }

        // GET api/analytics/user/{userId}/macros
        [HttpGet("user/{userId}/macros")]
        public async Task<IActionResult> GetMacroAnalytics(int userId)
        {
            try
            {
                var todayMacros = await analyticsRepo.GetTodayMacroTotalsAsync(userId);
                var proteinAvg = await analyticsRepo.Get7DayMacroAverageAsync(userId, "Protein");
                var fatAvg = await analyticsRepo.Get7DayMacroAverageAsync(userId, "Fat");
                var carbsAvg = await analyticsRepo.Get7DayMacroAverageAsync(userId, "Carbs");
                var caloriesAvg = await analyticsRepo.Get7DayMacroAverageAsync(userId, "Calories");
                var fibreAvg = await analyticsRepo.Get7DayMacroAverageAsync(userId, "Fibre");

                return Ok(new
                {
                    today = todayMacros,
                    sevenDayAverage = new
                    {
                        protein = Math.Round(proteinAvg, 1),
                        fat = Math.Round(fatAvg, 1),
                        carbs = Math.Round(carbsAvg, 1),
                        calories = Math.Round(caloriesAvg, 0),
                        fibre = Math.Round(fibreAvg, 1)
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving macro analytics: {ex.Message}");
            }
        }
    }
}
