using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodRepository foodRepo;
        public FoodController(IFoodRepository foodRepo)
        {
            this.foodRepo = foodRepo;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFoods() => Ok(await foodRepo.GetAllFoods());

        [HttpPost("create")]
        public async Task<IActionResult> CreateFood([FromBody] FoodItem newFood)
        {
            var created = await foodRepo.AddFood(newFood);
            return Ok(created);
        }

        [HttpGet("logs/{userId}/{date}")]
        public async Task<IActionResult> GetDailyLogs(int userId, DateTime date)
        {
            var logs = await foodRepo.GetLogsByDate(userId, date);
            return Ok(logs);
        }

        [HttpGet("weekly/{userId}/{startDate}")]
        public async Task<IActionResult> GetWeeklyLogs(int userId, DateTime startDate)
        {
            var logs = await foodRepo.GetWeeklyLogs(userId, startDate);
            return Ok(logs);
        }

        [HttpPost("log")]
        public async Task<IActionResult> PostFoodLog([FromBody] FoodLog newLog)
        {
            if (newLog == null)
            {
                return BadRequest("Data not recieved.");
            }
            try
            {
                await foodRepo.LogFood(newLog.UserID, DateTime.Now, newLog);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}