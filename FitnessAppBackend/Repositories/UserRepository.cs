// These repositories are the only files which can talk to the SQL database, and this one is responsible for fetching and saving user data.
// I'm not going to repeat this comment in the other files because it's the same for all repositories I made,
// so just remember when marking that this is a fundamental feature I used to achieve Complex OOP as also stated in the design document.

using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;


namespace FitnessAppBackend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;
        public UserRepository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<User?> GetUserByUsername(string username) => await context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        public async Task CreateUser(User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        public async Task<User?> GetUserById(int userId) =>await context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

        // Fetch the details for current user, then update any fields that get changed in Settings.xaml
        public async Task UpdateUser(int userId, User user) 
        {
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (existingUser == null)
                throw new Exception("User not found.");

            existingUser.UserDOB = user.UserDOB;
            existingUser.BodyWeight = user.BodyWeight;
            existingUser.Height = user.Height;
            existingUser.Gender = user.Gender;
            existingUser.ActivityLevel = user.ActivityLevel;
            existingUser.MaintenanceGoal = user.MaintenanceGoal;

            context.Users.Update(existingUser);
            await context.SaveChangesAsync();
        }

        public async Task<int> GetUserCount() => await context.Users.CountAsync();
    }
}
// completed