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
        private readonly IUserRepository _userRepo;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _hasher = new PasswordHasher<User>();
        }

        public record RegisterRequest(string UserName, string Password, DateOnly UserDOB, decimal BodyWeight, decimal Height, string Gender, string ActivityLevel);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request) // completed
        {
            // Validation:
            if (string.IsNullOrWhiteSpace(request.UserName) || request.UserName.Length > 50)
                return BadRequest("Username is required and must be <= 50 characters.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                return BadRequest("Password must be at least 8 characters.");

            if (await _userRepo.UserExistsAsync(request.UserName))
                return Conflict("Username already exists.");

            // Map new properties to the new User Table:
            var user = new User
            {
                UserName = request.UserName,
                UserDOB = request.UserDOB,
                BodyWeight = request.BodyWeight,
                Height = request.Height,
                Gender = request.Gender,
                ActivityLevel = request.ActivityLevel
            };
            
            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            // Save by using the Repository:
            await _userRepo.CreateUserAsync(user);
            return Ok("-> User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterRequest request)
        {
            var user = await _userRepo.GetUserByUsernameAsync(request.UserName);
            if (user == null)
                return Unauthorized("Invalid Username.");
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid Password.");
            return Ok(new { user.UserName, user.UserID } + " -> User logged in successfully.");
        }

        [HttpGet("fitness-app-db")]
        public async Task<IActionResult> TestDb()
        {
            var userCount = await _userRepo.GetUserCountAsync();
            return Ok($"Connected. Users in DB: {userCount}");
        }
    }
}