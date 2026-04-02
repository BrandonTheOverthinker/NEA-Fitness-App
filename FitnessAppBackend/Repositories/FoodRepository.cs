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

        public async Task<List<FoodItem>> GetAllFoods()
        {
            return await context.FoodItems.ToListAsync();
        }

        public async Task<FoodItem> AddFood(FoodItem newFood) // does what it says on the tin mate
        {
            context.FoodItems.Add(newFood);
            await context.SaveChangesAsync();
            return newFood;
        }

        public async Task<List<FoodLog>> GetLogsByDate(int userId, DateTime date)
        {
            // Get all the food logs to display in the the table in FoodLog.xaml:
            return await context.FoodLogs.Include(f => f.FoodItem).Where(f => f.UserID == userId && f.LogTime.Date == date.Date).ToListAsync();
        }

        public async Task<List<FoodLog>> GetWeeklyLogs(int userId, DateTime startDate)
        {
           DateTime endDate = startDate.AddDays(7);
           // Fetches all of the user's logged foods within the selected date window including full food item details
           return await context.FoodLogs.Include(f => f.FoodItem).Where(f => f.UserID == userId && f.LogTime >= startDate && f.LogTime < endDate).ToListAsync();
        }

        public async Task<List<FoodLog>> LogFood(int userId, DateTime date, FoodLog entry)
        {
            context.FoodLogs.Add(entry);
            await context.SaveChangesAsync();
            return await context.FoodLogs.Include(f => f.FoodItem).Where(f => f.UserID == userId && f.LogTime.Date == date.Date) .ToListAsync();
        }
    }
}