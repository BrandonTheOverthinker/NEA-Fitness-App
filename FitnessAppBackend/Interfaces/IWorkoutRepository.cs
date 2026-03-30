using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IWorkoutRepository
    {
        // Exercise Libraries:
        Task<List<Exercise>> GetAllExercisesAsync(); // Global exercise library
        Task<List<Exercise>> GetUserExercisesAsync(int userId); // User's personal exercise library
        Task<Exercise> CreateExerciseAsync(Exercise exercise, int userId); // Adds to global & user libraries
        Task AddExerciseToUserLibraryAsync(int userId, int exerciseId);

        // Workouts:
        Task<Workout> StartWorkoutAsync(Workout workout);
        Task FinishWorkoutAsync(int workoutId, int durationSeconds);

        // Exercise Logging (within a workout):
        Task<ExerciseLog> LogExerciseAsync(ExerciseLog log);
        Task<Set> LogSetAsync(Set set);

        // Analytics:
        Task<List<ExerciseLog>> GetExerciseHistoryAsync(int userId, int exerciseId); 
        Task<List<Set>> GetSetsForExerciseLogAsync(int exerciseLogId);

        // PR Tracking:
        Task<ExerciseLog> GetExerciseLogAsync(int exerciseLogId);
        Task<UserPersonalRecord?> CheckAndSavePRAsync(int userId, int exerciseId, Set set);
        Task DeleteExerciseFromWorkoutAsync(int exerciseLogId);
    }
}
