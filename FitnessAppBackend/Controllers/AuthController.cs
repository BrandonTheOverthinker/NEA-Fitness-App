using FitnessAppBackend.Data;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<FitnessAppBackend.Models.User> _hasher = new();

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        public record RegisterRequest(string UserName, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || request.UserName.Length > 50)
                return BadRequest("Username is required and must be <= 50 characters.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                return BadRequest("Password must be at least 8 characters.");

            bool exists = await _db.Users.AnyAsync(u => u.UserName == request.UserName);
            if (exists)
                return Conflict("Username already exists.");

            var user = new User
            {
                UserName = request.UserName
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Created("", new { user.UserID, user.UserName }); // Return minimal safe response
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null)
                return Unauthorized("Invalid Username.");
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid Password.");
            return Ok(new { user.UserName });
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDb()
        {
            var userCount = await _db.Users.CountAsync();
            return Ok($"Connected. Users in DB: {userCount}");
        }
    }
}