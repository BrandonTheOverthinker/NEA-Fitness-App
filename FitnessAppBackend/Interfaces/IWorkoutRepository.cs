using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IWorkoutRepository
    {
        // Exercise Libraries:
        Task<List<Exercise>> GetAllExercisesAsync(); // Global exercise library (all users)
        Task<List<Exercise>> GetUserExercisesAsync(int userId); // User's personal exercise library
        Task<Exercise> CreateExerciseAsync(Exercise exercise, int userId); // Add to global & user libraries
        Task AddExerciseToUserLibraryAsync(int userId, int exerciseId); // Add existing global exercise to user's library

        // Workouts:
        Task<Workout> StartWorkoutAsync(Workout workout); // Create workout row & return with generated ID
        Task FinishWorkoutAsync(int workoutId, int durationSeconds);

        // Exercise Logging (within a workout):
        Task<ExerciseLog> LogExerciseAsync(ExerciseLog log);
        Task<Set> LogSetAsync(Set set);

        // Analytics:
        Task<List<ExerciseLog>> GetExerciseHistoryAsync(int userId, int exerciseId); 
        Task<List<Set>> GetSetsForExerciseLogAsync(int exerciseLogId); // All sets for a specific exercise log

        // PR Tracking:
        Task<ExerciseLog> GetExerciseLogAsync(int exerciseLogId);
        Task<UserPersonalRecord?> CheckAndSavePRAsync(int userId, int exerciseId, Set set);
        Task DeleteExerciseFromWorkoutAsync(int exerciseLogId);
    }
}
