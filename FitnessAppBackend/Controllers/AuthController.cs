using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository userRepo;
        private readonly PasswordHasher<User> hasher = new();

        public AuthController(IUserRepository userRepo)
        {
            this.userRepo = userRepo;
            hasher = new PasswordHasher<User>();
        }

        public record RegisterRequest(int UserID, string UserName, string Password, DateOnly UserDOB, decimal BodyWeight, decimal Height, string Gender, string ActivityLevel, decimal MaintenanceGoal);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request) // Get info from RegisterRequest and map to User model for DB insertion
        {
            // Validation:
            //var errors = new List<string>();

            //if (string.IsNullOrWhiteSpace(request.UserName) || request.UserName.Length > 50)
            //    errors.Add("Username is required and must be <= 50 characters.");
            //if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            //    errors.Add("Password must be at least 8 characters.");
            //if (request.Height <= 0m || request.Height > 300.0m)
            //    errors.Add("Height must not be empty, and must be below 300cm.");
            //if (request.BodyWeight <= 0m || request.BodyWeight > 999.9m)
            //    errors.Add("Body Weight must not be empty, and must be below 1000kg.");
            //if (request.Gender == null)
            //    errors.Add("Please select a gender.");
            //if (request.ActivityLevel == null)
            //    errors.Add("Please select an Activity Level.");
            //if (await userRepo.UserExistsAsync(request.UserName))
            //    errors.Add("Username already taken.");
            //if (errors.Any())
            //    return BadRequest(new { errors });

            // Map new properties to the new User Table:
            var newUser = new User
            {
                UserID = request.UserID,
                UserName = request.UserName,
                UserDOB = request.UserDOB,
                BodyWeight = request.BodyWeight,
                Height = request.Height,
                Gender = request.Gender,
                ActivityLevel = request.ActivityLevel,
                MaintenanceGoal = request.MaintenanceGoal
            };
            
            newUser.PasswordHash = hasher.HashPassword(newUser, request.Password);

            // Save by using the Repository:
            await userRepo.CreateUserAsync(newUser);
            return Ok("-> User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterRequest request) // Get info from RegisterRequest and validate against DB records for authentication
        {
            var user = await userRepo.GetUserByUsernameAsync(request.UserName);
            // Validation:
            if (user == null)
                return Unauthorized("Invalid Username.");
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid Password.");

            return Ok(new
            {
                user.UserName,
                user.UserID,
                user.MaintenanceGoal,
                Message = "User logged in successfully."
            });
        }

        [HttpGet("fitness-app-db")]
        public async Task<IActionResult> CreateDb()
        {
            var userCount = await userRepo.GetUserCountAsync();
            return Ok($"Connected. Users in DB: {userCount}");
        }
    }
}