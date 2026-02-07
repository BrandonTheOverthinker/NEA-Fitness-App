using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(User user);
        Task<bool> UserExistsAsync(string username);
        Task<int> GetUserCountAsync();
    }
}
