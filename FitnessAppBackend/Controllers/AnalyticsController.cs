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

        [HttpGet("user/{userId}/summary")]
        public async Task<IActionResult> GetAnalyticsSummary(int userId)
        {
            try
            {
                var todayCalories = await analyticsRepo.GetTodayCalories(userId);
                var weeklyCalories = await analyticsRepo.GetWeeklyCalories(userId);
                var weeklyWorkouts = await analyticsRepo.GetWeeklyWorkoutCount(userId);
                var weeklyDuration = await analyticsRepo.GetWeeklyWorkoutDuration(userId);
                var highestFood = await analyticsRepo.GetHighestCalorieFoodToday(userId);
                var recentFood = await analyticsRepo.GetMostRecentFood(userId);
                var goalProgress = await analyticsRepo.GetGoalProgress(userId);

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

        [HttpGet("user/{userId}/macros")]
        public async Task<IActionResult> GetMacroAnalytics(int userId)
        {
            try
            {
                var todayMacros = await analyticsRepo.GetTodayMacroTotals(userId);
                var proteinAvg = await analyticsRepo.Get7DayMacroAverage(userId, "Protein");
                var fatAvg = await analyticsRepo.Get7DayMacroAverage(userId, "Fat");
                var carbsAvg = await analyticsRepo.Get7DayMacroAverage(userId, "Carbs");
                var caloriesAvg = await analyticsRepo.Get7DayMacroAverage(userId, "Calories");
                var fibreAvg = await analyticsRepo.Get7DayMacroAverage(userId, "Fibre");

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
