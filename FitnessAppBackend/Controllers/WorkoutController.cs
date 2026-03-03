using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutRepository workoutRepo;
        public WorkoutController(IWorkoutRepository workoutRepo) => this.workoutRepo = workoutRepo;

        // ── DTOs (Records) ────────────────────────────────────────────────────
        // Records are immutable value objects — ideal for API request shapes.
        // The frontend sends JSON matching these shapes.

        public record CreateExerciseRequest(string ExerciseName, string ExerciseType, int UserId);
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
            int TimeSeconds,
            decimal RPE
        );

        // GET api/workout/exercises/all
        [HttpGet("exercises/all")]
        public async Task<IActionResult> GetAllExercises() =>
            Ok(await workoutRepo.GetAllExercisesAsync());

        // GET api/workout/exercises/user/{userId}
        [HttpGet("exercises/user/{userId}")]
        public async Task<IActionResult> GetUserExercises(int userId) =>
            Ok(await workoutRepo.GetUserExercisesAsync(userId));

        // POST api/workout/exercises/create
        [HttpPost("exercises/create")]
        public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ExerciseName) || request.ExerciseName.Length > 50)
                return BadRequest("Exercise name is required and must be 50 characters or fewer.");

            Exercise exercise;
            if (request.ExerciseType == "Strength")
            {
                exercise = new StrengthExercise { ExerciseName = request.ExerciseName };
            }
            else if (request.ExerciseType == "Cardio")
            {
                exercise = new CardioExercise { ExerciseName = request.ExerciseName };
            }
            else
            {
                return BadRequest("ExerciseType must be 'Strength' or 'Cardio'.");
            }

            var created = await workoutRepo.CreateExerciseAsync(exercise, request.UserId);
            return Ok(created);
        }

        // POST api/workout/exercises/add-to-library
        // Add existing global exercise to user's personal library:
        [HttpPost("exercises/add-to-library")]
        public async Task<IActionResult> AddToLibrary([FromBody] (int UserId, int ExerciseId) request)
        {
            await workoutRepo.AddExerciseToUserLibraryAsync(request.UserId, request.ExerciseId);
            return Ok();
        }

        // POST api/workout/start
        // Create new Workout row:
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
        // Save final duration when user ends the session:
        [HttpPatch("{workoutId}/finish")]
        public async Task<IActionResult> FinishWorkout(int workoutId, [FromBody] FinishWorkoutRequest request)
        {
            if (request.DurationSeconds < 0)
                return BadRequest("Duration cannot be negative.");

            await workoutRepo.FinishWorkoutAsync(workoutId, request.DurationSeconds);
            return Ok();
        }

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
                TimeSeconds = request.TimeSeconds,
                RPE = request.RPE
            };

            var created = await workoutRepo.LogSetAsync(set);
            return Ok(created);
        }

        // GET api/workout/history/{userId}/{exerciseId}
        // Return all ExerciseLogs (with nested Sets) for one exercise:
        [HttpGet("history/{userId}/{exerciseId}")]
        public async Task<IActionResult> GetExerciseHistory(int userId, int exerciseId)
        {
            var history = await workoutRepo.GetExerciseHistoryAsync(userId, exerciseId);
            return Ok(history);
        }

        // GET api/workout/sets/{exerciseLogId}
        // Return all sets for a specific log entry:
        [HttpGet("sets/{exerciseLogId}")]
        public async Task<IActionResult> GetSetsForLog(int exerciseLogId)
        {
            var sets = await workoutRepo.GetSetsForExerciseLogAsync(exerciseLogId);
            return Ok(sets);
        }
    }
}
