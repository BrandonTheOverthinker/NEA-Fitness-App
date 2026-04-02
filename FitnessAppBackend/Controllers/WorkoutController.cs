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
        public WorkoutController(IWorkoutRepository workoutRepo)
        {
            this.workoutRepo = workoutRepo;
        }

        public record CreateExerciseRequest(string exerciseName, string exerciseType, int userId);
        public record AddToLibraryRequest(int userId, int exerciseId);
        public record StartWorkoutRequest(int userId, string workoutName, string workoutNotes);
        public record FinishWorkoutRequest(int durationSeconds);
        public record LogExerciseRequest(int workoutId, int userId, int exerciseId, string exerciseNotes);
        public record LogSetRequest(int exerciseLogId, int setNumber, string setType, // "Strength" or "Cardio"
            int reps, decimal setWeightKG, int distanceM, int timeSeconds);

        // Returns the full global exercise library thar all users can see
        [HttpGet("exercises/all")]
        public async Task<IActionResult> GetAllExercises() =>
            Ok(await workoutRepo.GetAllExercises());

        // Returns only the exercises in a user's personal library
        [HttpGet("exercises/user/{userId}")]
        public async Task<IActionResult> GetUserExercises(int userId) =>
            Ok(await workoutRepo.GetUserExercises(userId));

        // Creates a new exercise in the global DB and adds it to the user's library
        [HttpPost("exercises/create")]
        public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.exerciseName) || request.exerciseName.Length > 50)
                return BadRequest("Exercise name is required and must be 50 characters or fewer.");

            // Instantiate the correct subclass based on ExerciseType
            Exercise exercise = request.exerciseType == "Strength"
                ? new StrengthExercise { ExerciseName = request.exerciseName }
                : new CardioExercise { ExerciseName = request.exerciseName };

            var created = await workoutRepo.CreateExercise(exercise, request.userId);
            return Ok(created);
        }

        // Adds an existing global exercise to the user's personal library by fetching the ExerciseID from the frontend and passing it to the repo method
        [HttpPost("exercises/add-to-library")]
        public async Task<IActionResult> AddToLibrary([FromBody] AddToLibraryRequest request)
        {
            await workoutRepo.AddExerciseToUserLibrary(request.userId, request.exerciseId);
            return Ok();
        }

        // Creates a new Workout row; frontend uses the returned WorkoutID for the session
        [HttpPost("start")]
        public async Task<IActionResult> StartWorkout([FromBody] StartWorkoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.workoutName))
                return BadRequest("Workout name is required.");

            var workout = new Workout
            {
                UserID = request.userId,
                WorkoutName = request.workoutName,
                WorkoutNotes = request.workoutNotes ?? string.Empty
            };

            var created = await workoutRepo.StartWorkout(workout);
            return Ok(created);
        }

        [HttpPatch("{workoutId}/finish")]
        public async Task<IActionResult> FinishWorkout(int workoutId, [FromBody] FinishWorkoutRequest request)
        {
            if (request.durationSeconds < 0)
                return BadRequest("Duration cannot be negative.");

            await workoutRepo.FinishWorkout(workoutId, request.durationSeconds);
            return Ok();
        }

        // Adds an exercise to an active workout (creates an ExerciseLog row in ActiveWorkoutPage.xaml)
        [HttpPost("log-exercise")]
        public async Task<IActionResult> LogExercise([FromBody] LogExerciseRequest request)
        {
            var log = new ExerciseLog
            {
                WorkoutID = request.workoutId,
                UserID = request.userId,
                ExerciseID = request.exerciseId,
                ExerciseNotes = request.exerciseNotes ?? string.Empty
            };

            var created = await workoutRepo.LogExercise(log);
            return Ok(created);
        }

        // Adds a single set to an exercise log
        [HttpPost("log-set")]
        public async Task<IActionResult> LogSet([FromBody] LogSetRequest request)
        {
            var set = new Set
            {
                ExerciseLogID = request.exerciseLogId,
                SetNumber = request.setNumber,
                SetType = request.setType,
                Reps = request.reps,
                SetWeightKG = request.setWeightKG,
                DistanceM = request.distanceM,
                TimeSeconds = request.timeSeconds
            };

            var createdSet = await workoutRepo.LogSet(set);

            // Check for PR by fetching the ExerciseLog to get the UserID and ExerciseID,
            // then passing those along with the new set details to the repo method that checks for PRs and saves if it's a new one
            var exerciseLog = await workoutRepo.GetExerciseLog(request.exerciseLogId);
            var pr = await workoutRepo.CheckAndSavePR(exerciseLog.UserID, exerciseLog.ExerciseID, createdSet);

            return Ok(new
            {
                set = createdSet,
                isPR = pr != null,
                prInfo = pr != null ? new { prValue = pr.PRValue, prType = pr.PRType } : null
            });
        }

        [HttpDelete("log-exercise/{exerciseLogId}")]
        public async Task<IActionResult> DeleteExerciseFromWorkout(int exerciseLogId)
        {
            try
            {
                await workoutRepo.DeleteExerciseFromWorkout(exerciseLogId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting exercise from workout: {ex.Message}");
            }
        }

        [HttpGet("history/{userId}/{exerciseId}")]
        public async Task<IActionResult> GetExerciseHistory(int userId, int exerciseId)
        {
            var history = await workoutRepo.GetExerciseHistory(userId, exerciseId);
            return Ok(history);
        }

        [HttpGet("sets/{exerciseLogId}")]
        public async Task<IActionResult> GetSetsForLog(int exerciseLogId)
        {
            var sets = await workoutRepo.GetSetsForExerciseLog(exerciseLogId);
            return Ok(sets);
        }
    }
}