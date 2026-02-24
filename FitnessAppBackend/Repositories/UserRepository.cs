// These repositories are the only files which can talk to the SQL database, and this one is responsible for fetching and saving user data.
// Implements the IUserRepository interface, which defines the contract for user-related operations.
// Uses Entity Framework Core to interact with the database, allowing for asynchronous operations to improve performance and responsiveness.

using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;
        public UserRepository(AppDbContext context) => this.context = context;

        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        public async Task CreateUserAsync(User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(string username) =>
            await context.Users.AnyAsync(u => u.UserName == username);

        public async Task<int> GetUserCountAsync() =>
            await context.Users.CountAsync();
    }
}
// completed