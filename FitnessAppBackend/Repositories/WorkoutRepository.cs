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
        public WorkoutRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Exercise>> GetAllExercises() => await context.Exercises.ToListAsync();

        // Retrieves all exercises the user has added to their profile by joining their exercises with the exercise library
        public async Task<List<Exercise>> GetUserExercises(int userId) => await context.UserExercises.Where(ue => ue.UserID == userId).Join(context.Exercises, ue => ue.ExerciseID, e => e.ExerciseID, (ue, e) => e).ToListAsync();

        public async Task<Exercise> CreateExercise(Exercise exercise, int userId) // Does what it says on the tin mate
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

        // Let user add an exercise from global library to their personal library
        public async Task AddExerciseToUserLibrary(int userId, int exerciseId)
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

        public async Task<Workout> StartWorkout(Workout workout) // This would make a new row in ActiveWorkoutPage.xaml
        {
            workout.WorkoutTime = DateTime.UtcNow;
            context.Workouts.Add(workout);
            await context.SaveChangesAsync();
            return workout;
        }

        public async Task FinishWorkout(int workoutId, int durationSeconds)
        {
            var workout = await context.Workouts.FindAsync(workoutId);
            if (workout != null)
            {
                workout.WorkoutDurationS = durationSeconds;
                await context.SaveChangesAsync();
            }
        }

        public async Task<ExerciseLog> LogExercise(ExerciseLog log)
        {
            // Auto-assigns order based on how many exercises already exist in this workout
            int currentCount = await context.ExerciseLogs.CountAsync(el => el.WorkoutID == log.WorkoutID);
            log.ExerciseOrder = currentCount + 1;

            context.ExerciseLogs.Add(log);
            await context.SaveChangesAsync();
            return log;
        }

        public async Task<Set> LogSet(Set set)
        {
            context.Sets.Add(set);
            await context.SaveChangesAsync();
            return set;
        }


        // Pulls every instance where the user performed this exercise so the analytics page can chart their progress over time

        public async Task<List<ExerciseLog>> GetExerciseHistory(int userId, int exerciseId) => await context.ExerciseLogs
                .Include(el => el.Workout)   // For the date on the X-axis
                .Include(el => el.Exercise)  // For the exercise name heading
                .Where(el => el.UserID == userId && el.ExerciseID == exerciseId).OrderBy(el => el.Workout!.WorkoutTime).ToListAsync();

        public async Task<List<Set>> GetSetsForExerciseLog(int exerciseLogId) => await context.Sets .Where(s => s.ExerciseLogID == exerciseLogId).OrderBy(s => s.SetNumber).ToListAsync();

        public async Task<UserPersonalRecord?> CheckAndSavePR(int userId, int exerciseId, Set set)
        {
            // This is the actual PR check that fetches the user's current exercise data:
            var existingPR = await context.PersonalRecords.Where(pr => pr.UserID == userId && pr.ExerciseID == exerciseId).FirstOrDefaultAsync();

            bool isPR = false;
            string prType = set.SetType == "Strength" ? "Weight" : "Distance";

            if (set.SetType == "Strength")
            {
                // For strength PR should be highest weight lifted:
                if (existingPR == null || set.SetWeightKG > existingPR.PRValue)
                    isPR = true;
            }
            else
            {
                // For cardio PR it's furthest distance:
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

        public async Task<ExerciseLog> GetExerciseLog(int exerciseLogId) =>
            await context.ExerciseLogs.Include(el => el.Workout).Include(el => el.Exercise).Include(el => el.User).FirstOrDefaultAsync(el => el.ExerciseLogID == exerciseLogId) 
            ?? throw new Exception("Exercise log not found.");

        public async Task DeleteExerciseFromWorkout(int exerciseLogId)
        {
            var log = await context.ExerciseLogs.FindAsync(exerciseLogId);
            if (log == null) return;

            int workoutId = log.WorkoutID;

            // Remove sets for this log, then the for the exercise log and then order them:

            var sets = context.Sets.Where(s => s.ExerciseLogID == exerciseLogId);
            context.Sets.RemoveRange(sets);

            context.ExerciseLogs.Remove(log);
            await context.SaveChangesAsync();

            var remaining = await context.ExerciseLogs.Where(el => el.WorkoutID == workoutId).OrderBy(el => el.ExerciseOrder).ToListAsync();
            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].ExerciseOrder = i + 1;
            }

            await context.SaveChangesAsync();
        }
    }
}