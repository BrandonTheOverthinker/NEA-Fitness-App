using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly AppDbContext context;

        public FoodRepository(AppDbContext context)
        {
            this.context = context;
        }

        // Logic to find public items OR items created by the specific user:
        public async Task<IEnumerable<FoodItem>> SearchFoodsAsync(string query, int userId)
        {
            return await context.FoodItems
                .Where(f => (f.CreatedByUserID == null || f.CreatedByUserID == userId) && f.FoodName.Contains(query)).ToListAsync();
        }

        // Used for the potential Barcode Scanner feature:
        public async Task<FoodItem?> GetFoodByBarcodeAsync(string barcode)
        {
            return await context.FoodItems
                .FirstOrDefaultAsync(f => f.Barcode == barcode);
        }

        // Adds a new food definition to the library (Public or Private):
        public async Task AddFoodItemAsync(FoodItem foodItem)
        {
            context.FoodItems.Add(foodItem);
            await context.SaveChangesAsync();
        }

        // Records a meal entry in the FoodLog table:
        public async Task LogFoodAsync(FoodLog log)
        {
            context.FoodLogs.Add(log);
            await context.SaveChangesAsync();
        }

        // Fetches all logs for a specific day to calculate totals on the dashboard:
        public async Task<IEnumerable<FoodLog>> GetDailyLogsAsync(int userId, DateTime date)
        {
            return await context.FoodLogs
                .Include(l => l.FoodItem) // Joins the food data so you have the macros
                .Where(l => l.UserID == userId && l.LogTime.Date == date.Date)
                .ToListAsync();
        }
    }
}