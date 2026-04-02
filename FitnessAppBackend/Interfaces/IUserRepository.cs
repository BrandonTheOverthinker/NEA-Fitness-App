using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsername(string username);
        Task CreateUser(User user);
        Task<User?> GetUserById(int userId);
        Task UpdateUser(int userId, User user);
        Task<int> GetUserCount();
    }
}