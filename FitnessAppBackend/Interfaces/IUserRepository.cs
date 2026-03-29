using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int userId);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(int userId, User user);
        Task<bool> UserExistsAsync(string username);
        Task<int> GetUserCountAsync();
    }
}