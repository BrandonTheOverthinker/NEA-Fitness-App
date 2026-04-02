using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IGoalRepository
    {
        Task<UserGoal> CreateWeightGoal(int userId, string description, DateTime deadline, decimal targetWeight, decimal startWeight);
        Task<UserGoal> CreateExerciseGoal(int userId, string description, DateTime deadline, int exerciseId, decimal targetWeight, int targetTime);
        Task<List<UserGoal>> GetUserGoals(int userId);
        Task<UserGoal?> GetGoalById(int goalId);
        Task CompleteGoal(int goalId);
        Task DeleteGoal(int goalId);
        Task<NutritionGoal> CreateNutritionGoal(int weightGoalId, int calorieGoal, decimal proteinGoal, decimal fatGoal, decimal satFatGoal, decimal carbsGoal, decimal sugarGoal, decimal fibreGoal);
        Task<WeightGoal?> GetWeightGoalByGoalId(int goalId);
        Task<ExerciseGoal?> GetExerciseGoalByGoalId(int goalId);
    }
}
