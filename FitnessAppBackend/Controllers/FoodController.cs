using FitnessAppBackend.Interfaces;
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

        [HttpGet("search/{userId}")]
        public async Task<IActionResult> Search(int userId, [FromQuery] string query)
        {
            // Get the data from the Repository:
            var results = await foodRepo.SearchFoodsAsync(query, userId);

            return Ok(results);
        }
    }
}
