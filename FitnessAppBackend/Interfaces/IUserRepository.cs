using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IUserRepository // tells the controller what methods the repository must implement, but not how they work
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(User user);
        Task<bool> UserExistsAsync(string username);
        Task<int> GetUserCountAsync();
    }
}
// completed