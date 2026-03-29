using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IGoalRepository
    {
        Task<UserGoal> CreateWeightGoalAsync(int userId, string description, DateTime deadline, decimal targetWeight, decimal startWeight);
        Task<UserGoal> CreateExerciseGoalAsync(int userId, string description, DateTime deadline, int exerciseId, decimal targetWeight, int targetTime);
        Task<List<UserGoal>> GetUserGoalsAsync(int userId);
        Task<UserGoal?> GetGoalByIdAsync(int goalId);
        Task CompleteGoalAsync(int goalId);
        Task DeleteGoalAsync(int goalId);
        Task<NutritionGoal> CreateNutritionGoalAsync(int weightGoalId, int calorieGoal, decimal proteinGoal, decimal fatGoal, decimal satFatGoal, decimal carbsGoal, decimal sugarGoal, decimal fibreGoal);
        Task<WeightGoal?> GetWeightGoalByGoalIdAsync(int goalId);
        Task<ExerciseGoal?> GetExerciseGoalByGoalIdAsync(int goalId);
    }
}
