using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IFoodRepository
    {
        Task<List<FoodItem>> GetAllFoodsAsync();
        Task<FoodItem> AddFoodAsync(FoodItem newFood);
        Task<List<FoodLog>> GetLogsByDateAsync(int userId, DateTime date);
        Task<List<FoodLog>> GetWeeklyLogsAsync(int userId, DateTime startDate);
        Task<List<FoodLog>> LogFoodAsync(int userId, DateTime date, FoodLog entry);
    }
}