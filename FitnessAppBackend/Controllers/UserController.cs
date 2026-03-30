using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepo;
        public UserController(IUserRepository userRepo) => this.userRepo = userRepo;

        public record UpdateUserRequest(DateOnly UserDOB, decimal BodyWeight, decimal Height, string Gender, string ActivityLevel, decimal MaintenanceGoal);

        // GET api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(new
            {
                user.UserID,
                user.UserName,
                user.UserDOB,
                user.BodyWeight,
                user.Height,
                user.Gender,
                user.ActivityLevel,
                user.MaintenanceGoal
            });
        }

        // PUT api/user/{id}
        // Update profile fields without requiring password/PasswordHash in the request:
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var existing = await userRepo.GetUserByIdAsync(id);
            if (existing == null) return NotFound();

            existing.UserDOB = request.UserDOB;
            existing.BodyWeight = request.BodyWeight;
            existing.Height = request.Height;
            existing.Gender = request.Gender;
            existing.ActivityLevel = request.ActivityLevel;
            existing.MaintenanceGoal = request.MaintenanceGoal;

            await userRepo.UpdateUserAsync(id, existing);

            return Ok(new { Message = "Profile updated" });
        }
    }
}