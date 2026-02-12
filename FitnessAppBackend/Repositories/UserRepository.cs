// Only file which can talk to the SQL database, and is responsible for fetching and saving user data.
// Implements the IUserRepository interface, which defines the contract for user-related operations
// Uses Entity Framework Core to interact with the database, allowing for asynchronous operations to improve performance and responsiveness

using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(string username) =>
            await _context.Users.AnyAsync(u => u.UserName == username);

        public async Task<int> GetUserCountAsync() =>
            await _context.Users.CountAsync();
        
    }
}
// completed