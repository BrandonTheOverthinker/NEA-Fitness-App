using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IFoodRepository
    {
        Task<List<FoodItem>> GetAllFoods();
        Task<FoodItem> AddFood(FoodItem newFood);
        Task<List<FoodLog>> GetLogsByDate(int userId, DateTime date);
        Task<List<FoodLog>> GetWeeklyLogs(int userId, DateTime startDate);
        Task<List<FoodLog>> LogFood(int userId, DateTime date, FoodLog entry);
    }
}