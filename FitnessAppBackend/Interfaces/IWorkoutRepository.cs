using FitnessAppBackend.Models;

namespace FitnessAppBackend.Interfaces
{
    public interface IWorkoutRepository
    {
        // Exercise Libraries:
        Task<List<Exercise>> GetAllExercises(); // exercise library  for all users
        Task<List<Exercise>> GetUserExercises(int userId); // User's personal exercises
        Task<Exercise> CreateExercise(Exercise exercise, int userId); // Add to both the libraries
        Task AddExerciseToUserLibrary(int userId, int exerciseId);

        // Workouts:
        Task<Workout> StartWorkout(Workout workout);
        Task FinishWorkout(int workoutId, int durationSeconds);
            // Exercise Logging:
            Task<ExerciseLog> LogExercise(ExerciseLog log);
            Task<Set> LogSet(Set set);
                // PR Tracking:
                Task<ExerciseLog> GetExerciseLog(int exerciseLogId);
                Task<UserPersonalRecord?> CheckAndSavePR(int userId, int exerciseId, Set set);
                Task DeleteExerciseFromWorkout(int exerciseLogId);


        // Analytics:
        Task<List<ExerciseLog>> GetExerciseHistory(int userId, int exerciseId); 
        Task<List<Set>> GetSetsForExerciseLog(int exerciseLogId);


    }
}
