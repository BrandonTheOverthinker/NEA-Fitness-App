using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodRepository _foodRepo;
        public FoodController(IFoodRepository foodRepo) { _foodRepo = foodRepo; }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFoods() => Ok(await _foodRepo.GetAllFoodsAsync());

        [HttpPost("create")]
        public async Task<IActionResult> CreateFood([FromBody] FoodItem newFood)
        {
            var created = await _foodRepo.AddFoodAsync(newFood);
            return Ok(created);
        }

        [HttpGet("logs/{userId}/{date}")]
        public async Task<IActionResult> GetDailyLogs(int userId, DateTime date)
        {
            var logs = await _foodRepo.GetLogsByDateAsync(userId, date);
            return Ok(logs);
        }
        [HttpGet("weekly/{userId}/{startDate}")]
        public async Task<IActionResult> GetWeeklyLogs(int userId, DateTime startDate)
        {
            // This calls your repository method we fixed earlier
            var logs = await _foodRepo.GetWeeklyLogsAsync(userId, startDate);
            return Ok(logs);
        }
    }
}
