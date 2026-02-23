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
            var results = await foodRepo.SearchFoodsAsync(query, userId);

            // Convert to List for the sorting algorithm
            var list = results.ToList();

            // apply Merge Sort here
            //var sortedList = SortingHelper.MergeSort(list);
            var sortedList = 0; // placeholder for the sorted list  
            return Ok(sortedList);
        }
    }
}
