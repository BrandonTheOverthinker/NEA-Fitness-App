using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly AppDbContext context;

        public FoodRepository(AppDbContext context) => this.context = context;

        public async Task<List<FoodItem>> GetAllFoodsAsync()
        {
            return await context.FoodItems.ToListAsync();
        }

        public async Task<FoodItem> AddFoodAsync(FoodItem newFood)
        {
            context.FoodItems.Add(newFood);
            await context.SaveChangesAsync();
            return newFood;
        }

        public async Task<List<FoodLog>> GetLogsByDateAsync(int userId, DateTime date)
        {
            // Fixed casing: UserID instead of UserId
            return await context.FoodLogs
                .Include(f => f.FoodItem)
                .Where(f => f.UserID == userId && f.LogTime.Date == date.Date)
                .ToListAsync();
        }

        // Implementing the missing Interface member
        public async Task<List<FoodLog>> GetWeeklyLogsAsync(int userId, DateTime startDate)
        {
            DateTime endDate = startDate.AddDays(7);
            
            return await context.FoodLogs
                .Include(f => f.FoodItem)
                .Where(f => f.UserID == userId && f.LogTime >= startDate && f.LogTime < endDate)
                .ToListAsync();
        }
    }
}