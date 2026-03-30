// Only file with direct DB access for workout-related operations
using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAppBackend.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly AppDbContext context;
        public WorkoutRepository(AppDbContext context) => this.context = context;

        // Return all exercises in the global database:
        public async Task<List<Exercise>> GetAllExercisesAsync() =>
            await context.Exercises.ToListAsync();

        // Return personal exercise library:
        public async Task<List<Exercise>> GetUserExercisesAsync(int userId) =>
            await context.UserExercises
                .Where(ue => ue.UserID == userId)
                .Join(context.Exercises,
                    ue => ue.ExerciseID,
                    e => e.ExerciseID,
                    (ue, e) => e)
                .ToListAsync();

        public async Task<Exercise> CreateExerciseAsync(Exercise exercise, int userId) // Does what it says on the tin mate
        {
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync(); // Generate ExerciseID

            // Link the new exercise to the user's personal library:
            context.UserExercises.Add(new UserExercise
            {
                UserID = userId,
                ExerciseID = exercise.ExerciseID
            });
            await context.SaveChangesAsync();

            return exercise;
        }

        // Let user add an exercise from global library to their personal library:
        public async Task AddExerciseToUserLibraryAsync(int userId, int exerciseId)
        {
            bool alreadyAdded = await context.UserExercises
                .AnyAsync(ue => ue.UserID == userId && ue.ExerciseID == exerciseId);

            if (!alreadyAdded)
            {
                context.UserExercises.Add(new UserExercise
                {
                    UserID = userId,
                    ExerciseID = exerciseId
                });
                await context.SaveChangesAsync();
            }
        }

        // Create a new Workout row:
        public async Task<Workout> StartWorkoutAsync(Workout workout)
        {
            workout.WorkoutTime = DateTime.UtcNow;
            context.Workouts.Add(workout);
            await context.SaveChangesAsync(); // Generate WorkoutID
            return workout;
        }

        // Save elapsed timer value:
        public async Task FinishWorkoutAsync(int workoutId, int durationSeconds)
        {
            var workout = await context.Workouts.FindAsync(workoutId);
            if (workout != null)
            {
                workout.WorkoutDurationS = durationSeconds;
                await context.SaveChangesAsync();
            }
        }

        // Add exercise entry to active workout:
        public async Task<ExerciseLog> LogExerciseAsync(ExerciseLog log)
        {
            // Auto-assign order based on how many exercises already exist in this workout
            int currentCount = await context.ExerciseLogs
                .CountAsync(el => el.WorkoutID == log.WorkoutID);
            log.ExerciseOrder = currentCount + 1;

            context.ExerciseLogs.Add(log);
            await context.SaveChangesAsync();
            return log;
        }

        public async Task<Set> LogSetAsync(Set set)
        {
            context.Sets.Add(set);
            await context.SaveChangesAsync();
            return set;
        }


        // For analytics:

        // Return all times user has logged a specific exercise across all workouts:
        public async Task<List<ExerciseLog>> GetExerciseHistoryAsync(int userId, int exerciseId) => // Used to populate the exercise history table and chart on the analytics page.
            await context.ExerciseLogs
                .Include(el => el.Workout)   // For the date on the X-axis
                .Include(el => el.Exercise)  // For the exercise name heading
                .Where(el => el.UserID == userId && el.ExerciseID == exerciseId)
                .OrderBy(el => el.Workout!.WorkoutTime) // Chronological for chart
                .ToListAsync();

        // Return all sets for a specific exercise log entry (used to expand detail):
        public async Task<List<Set>> GetSetsForExerciseLogAsync(int exerciseLogId) =>
            await context.Sets
                .Where(s => s.ExerciseLogID == exerciseLogId)
                .OrderBy(s => s.SetNumber)
                .ToListAsync();

        // Check for PRs when logging a set:
        public async Task<UserPersonalRecord?> CheckAndSavePRAsync(int userId, int exerciseId, Set set)
        {
            // Check if this is a PR for this user/exercise:
            var existingPR = await context.PersonalRecords
                .Where(pr => pr.UserID == userId && pr.ExerciseID == exerciseId)
                .FirstOrDefaultAsync();

            bool isPR = false;
            string prType = set.SetType == "Strength" ? "Weight" : "Distance";

            if (set.SetType == "Strength")
            {
                // For strength, PR is the highest weight lifted:
                if (existingPR == null || set.SetWeightKG > existingPR.PRValue)
                    isPR = true;
            }
            else
            {
                // For cardio, PR is the furthest distance:
                if (existingPR == null || set.DistanceM > existingPR.PRValue)
                    isPR = true;
            }

            if (isPR)
            {
                decimal prValue = set.SetType == "Strength" ? set.SetWeightKG : set.DistanceM;

                var newPR = new UserPersonalRecord
                {
                    UserID = userId,
                    ExerciseID = exerciseId,
                    PRType = prType,
                    PRValue = prValue,
                    AchievedAt = DateTime.UtcNow,
                    SetID = set.SetID
                };

                if (existingPR != null)
                    context.PersonalRecords.Remove(existingPR);

                context.PersonalRecords.Add(newPR);
                await context.SaveChangesAsync();

                return newPR;
            }

            return null;
        }

        public async Task<ExerciseLog> GetExerciseLogAsync(int exerciseLogId) =>
            await context.ExerciseLogs
                .Include(el => el.Workout)
                .Include(el => el.Exercise)
                .Include(el => el.User)
                .FirstOrDefaultAsync(el => el.ExerciseLogID == exerciseLogId) 
            ?? throw new Exception("Exercise log not found.");

        public async Task DeleteExerciseFromWorkoutAsync(int exerciseLogId)
        {
            var log = await context.ExerciseLogs.FindAsync(exerciseLogId);
            if (log == null) return;

            int workoutId = log.WorkoutID;

            // Remove sets for this log:
            var sets = context.Sets.Where(s => s.ExerciseLogID == exerciseLogId);
            context.Sets.RemoveRange(sets);

            // Remove the exercise log:
            context.ExerciseLogs.Remove(log);
            await context.SaveChangesAsync();

            // Re-order remaining exercise logs for that workout:
            var remaining = await context.ExerciseLogs
                .Where(el => el.WorkoutID == workoutId)
                .OrderBy(el => el.ExerciseOrder)
                .ToListAsync();

            for (int i = 0; i < remaining.Count; i++)
                remaining[i].ExerciseOrder = i + 1;

            await context.SaveChangesAsync();
        }
    }
}