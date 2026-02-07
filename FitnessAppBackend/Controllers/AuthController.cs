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

        public record RegisterRequest(string UserName, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || request.UserName.Length > 50)
                return BadRequest("Username is required and must be <= 50 characters.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                return BadRequest("Password must be at least 8 characters.");

            bool exists = await _userRepo.UserExistsAsync(request.UserName);
            if (exists)
                return Conflict("Username already exists.");

            var user = new User
            {
                UserName = request.UserName
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            await _userRepo.CreateUserAsync(user);
            return Ok(new { user.UserName, user.UserID });
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
            return Ok(new { user.UserName, user.UserID });
        }

        [HttpGet("fitness-app-db")]
        public async Task<IActionResult> TestDb()
        {
            var userCount = await _userRepo.GetUserCountAsync();
            return Ok($"Connected. Users in DB: {userCount}");
        }
    }
}