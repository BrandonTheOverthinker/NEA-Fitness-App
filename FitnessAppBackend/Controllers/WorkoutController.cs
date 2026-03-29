using Azure.Core;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutRepository workoutRepo;
        public WorkoutController(IWorkoutRepository workoutRepo) => this.workoutRepo = workoutRepo;

        public record CreateExerciseRequest(string ExerciseName, string ExerciseType, int UserId);
        public record AddToLibraryRequest(int UserId, int ExerciseId);
        public record StartWorkoutRequest(int UserId, string WorkoutName, string WorkoutNotes);
        public record FinishWorkoutRequest(int DurationSeconds);
        public record LogExerciseRequest(int WorkoutId, int UserId, int ExerciseId, string ExerciseNotes);
        public record LogSetRequest(
            int ExerciseLogId,
            int SetNumber,
            string SetType, // "Strength" or "Cardio"
            int Reps,
            decimal SetWeightKG,
            int DistanceM,
            int TimeSeconds
        );

        // Exercise Library Endpoints:

        // GET api/workout/exercises/all
        // Returns the full global exercise library (all users can see)
        [HttpGet("exercises/all")]
        public async Task<IActionResult> GetAllExercises() =>
            Ok(await workoutRepo.GetAllExercisesAsync());

        // GET api/workout/exercises/user/{userId}
        // Returns only the exercises in a user's personal library
        [HttpGet("exercises/user/{userId}")]
        public async Task<IActionResult> GetUserExercises(int userId) =>
            Ok(await workoutRepo.GetUserExercisesAsync(userId));

        // POST api/workout/exercises/create
        // Creates a new exercise in the global DB and adds it to the user's library
        [HttpPost("exercises/create")]
        public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ExerciseName) || request.ExerciseName.Length > 50)
                return BadRequest("Exercise name is required and must be 50 characters or fewer.");

            // Polymorphism in action: we instantiate the correct subclass based on ExerciseType
            Exercise exercise = request.ExerciseType == "Strength"
                ? new StrengthExercise { ExerciseName = request.ExerciseName }
                : new CardioExercise { ExerciseName = request.ExerciseName };

            var created = await workoutRepo.CreateExerciseAsync(exercise, request.UserId);
            return Ok(created);
        }

        // POST api/workout/exercises/add-to-library
        // Adds an existing global exercise to the user's personal library
        [HttpPost("exercises/add-to-library")]
        public async Task<IActionResult> AddToLibrary([FromBody] AddToLibraryRequest request)
        {
            await workoutRepo.AddExerciseToUserLibraryAsync(request.UserId, request.ExerciseId);
            return Ok();
        }

        // Workout Endpoints:

        // POST api/workout/start
        // Creates a new Workout row; frontend uses the returned WorkoutID for the session
        [HttpPost("start")]
        public async Task<IActionResult> StartWorkout([FromBody] StartWorkoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.WorkoutName))
                return BadRequest("Workout name is required.");

            var workout = new Workout
            {
                UserID = request.UserId,
                WorkoutName = request.WorkoutName,
                WorkoutNotes = request.WorkoutNotes ?? string.Empty
            };

            var created = await workoutRepo.StartWorkoutAsync(workout);
            return Ok(created);
        }

        // PATCH api/workout/{workoutId}/finish
        // Saves the final duration when the user ends the session
        [HttpPatch("{workoutId}/finish")]
        public async Task<IActionResult> FinishWorkout(int workoutId, [FromBody] FinishWorkoutRequest request)
        {
            if (request.DurationSeconds < 0)
                return BadRequest("Duration cannot be negative.");

            await workoutRepo.FinishWorkoutAsync(workoutId, request.DurationSeconds);
            return Ok();
        }

        // Exercise Log Endpoints:

        // POST api/workout/log-exercise
        // Adds an exercise to an active workout (creates an ExerciseLog row)
        [HttpPost("log-exercise")]
        public async Task<IActionResult> LogExercise([FromBody] LogExerciseRequest request)
        {
            var log = new ExerciseLog
            {
                WorkoutID = request.WorkoutId,
                UserID = request.UserId,
                ExerciseID = request.ExerciseId,
                ExerciseNotes = request.ExerciseNotes ?? string.Empty
            };

            var created = await workoutRepo.LogExerciseAsync(log);
            return Ok(created);
        }

        // POST api/workout/log-set
        // Adds a single set to an exercise log
        [HttpPost("log-set")]
        public async Task<IActionResult> LogSet([FromBody] LogSetRequest request)
        {
            var set = new Set
            {
                ExerciseLogID = request.ExerciseLogId,
                SetNumber = request.SetNumber,
                SetType = request.SetType,
                Reps = request.Reps,
                SetWeightKG = request.SetWeightKG,
                DistanceM = request.DistanceM,
                TimeSeconds = request.TimeSeconds
            };

            var createdSet = await workoutRepo.LogSetAsync(set);

            // Check for PR
            var exerciseLog = await workoutRepo.GetExerciseLogAsync(request.ExerciseLogId);
            var pr = await workoutRepo.CheckAndSavePRAsync(exerciseLog.UserID, exerciseLog.ExerciseID, createdSet);

            return Ok(new
            {
                set = createdSet,
                isPR = pr != null,
                prInfo = pr != null ? new { prValue = pr.PRValue, prType = pr.PRType } : null
            });
        }

        // DELETE api/workout/log-exercise/{exerciseLogId}
        // Remove an exercise from an active workout:
        [HttpDelete("log-exercise/{exerciseLogId}")]
        public async Task<IActionResult> DeleteExerciseFromWorkout(int exerciseLogId)
        {
            try
            {
                await workoutRepo.DeleteExerciseFromWorkoutAsync(exerciseLogId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting exercise from workout: {ex.Message}");
            }
        }

        // Analytics Endpoints:

        // GET api/workout/history/{userId}/{exerciseId}
        // Returns all ExerciseLogs (with nested Sets) for one exercise - used for the chart + table
        [HttpGet("history/{userId}/{exerciseId}")]
        public async Task<IActionResult> GetExerciseHistory(int userId, int exerciseId)
        {
            var history = await workoutRepo.GetExerciseHistoryAsync(userId, exerciseId);
            return Ok(history);
        }

        // GET api/workout/sets/{exerciseLogId}
        // Returns all sets for a specific log entry
        [HttpGet("sets/{exerciseLogId}")]
        public async Task<IActionResult> GetSetsForLog(int exerciseLogId)
        {
            var sets = await workoutRepo.GetSetsForExerciseLogAsync(exerciseLogId);
            return Ok(sets);
        }
    }
}