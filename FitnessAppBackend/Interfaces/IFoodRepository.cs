using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IFoodRepository
    {
        Task<IEnumerable<FoodItem>> SearchFoodsAsync(string query, int userId);
        Task<FoodItem> GetFoodByBarcodeAsync(string barcode);
        Task AddFoodItemAsync(FoodItem foodItem);
        Task LogFoodAsync(FoodLog log);
        Task<IEnumerable<FoodLog>> GetDailyLogsAsync(int userId, DateTime date);
    }
}
