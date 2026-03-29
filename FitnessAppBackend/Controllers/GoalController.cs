using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FitnessAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoalController : ControllerBase
    {
        private readonly IGoalRepository goalRepo;
        private readonly IUserRepository userRepo;
        private readonly IWorkoutRepository workoutRepo;
    
        public GoalController(IGoalRepository goalRepo, IUserRepository userRepo)
        {
            this.goalRepo = goalRepo;
            this.userRepo = userRepo;
        }

        // DTOs
        public record CreateWeightGoalRequest(
            int UserId,
            string Description,
            DateTime Deadline,
            decimal TargetWeight,
            decimal StartWeight
        );

        public record CreateExerciseGoalRequest(
            int UserId,
            string Description,
            DateTime Deadline,
            int ExerciseId,
            decimal TargetWeight,
            int TargetTime
        );

        public record UserGoalResponse(
            int GoalID,
            int UserID,
            string GoalType,
            string GoalDescription,
            DateTime DateCreated,
            bool IsCompleted,
            int DaysUntilDeadline,
            WeightGoalData? WeightGoalData,
            ExerciseGoalData? ExerciseGoalData
        );

        public record WeightGoalData(int WGoalID, decimal TargetBW, decimal StartBW);

        public record ExerciseGoalData(int EGoalID, int ExerciseID, string ExerciseName, decimal TargetWeight, int TargetTime);

        // POST api/goals/weight
        [HttpPost("weight")]
        public async Task<IActionResult> CreateWeightGoal([FromBody] CreateWeightGoalRequest request)
        {
            try
            {
                if (request.TargetWeight <= 0 || request.StartWeight <= 0)
                    return BadRequest("Target and start weights must be positive.");

                var goal = await goalRepo.CreateWeightGoalAsync(
                    request.UserId,
                    request.Description,
                    request.Deadline,
                    request.TargetWeight,
                    request.StartWeight
                );

                // Try to calculate and create nutrition goal, but do not fail the entire request if it errors.
                try
                {
                    decimal calorieDeficit = CalculateCalorieDeficit(request.TargetWeight, request.StartWeight, request.Deadline);
                    var user = await userRepo.GetUserByIdAsync(request.UserId);
                    if (user != null)
                    {
                        decimal maintenanceGoal = user.MaintenanceGoal;
                        int targetCalories = Math.Max(1200, (int)(maintenanceGoal - Math.Abs(calorieDeficit)));

                        var weightGoal = await goalRepo.GetWeightGoalByGoalIdAsync(goal.GoalID);
                        if (weightGoal != null)
                        {
                            try
                            {
                                await goalRepo.CreateNutritionGoalAsync(
                                    weightGoal.WGoalID,
                                    targetCalories,
                                    targetCalories * 0.3m / 4, // 30% protein
                                    targetCalories * 0.3m / 9, // 30% fat
                                    targetCalories * 0.1m / 9, // 10% saturated fat
                                    targetCalories * 0.4m / 4, // 40% carbs
                                    targetCalories * 0.05m / 4, // 5% sugar
                                    30 // 30g fibre
                                );
                            }
                            catch (Exception exInner)
                            {
                                // Log and continue — nutrition goal persistence failing should not break weight goal creation.
                                Debug.WriteLine($"Failed to save NutritionGoal for WGoalID={weightGoal.WGoalID}: {exInner}");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"User not found when creating nutrition goal for UserId={request.UserId}");
                    }
                }
                catch (Exception exCalc)
                {
                    Debug.WriteLine($"Nutrition goal creation skipped due to error: {exCalc}");
                }

                return Ok(goal);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating goal: {ex.Message}");
            }
        }

        // POST api/goals/exercise
        [HttpPost("exercise")]
        public async Task<IActionResult> CreateExerciseGoal([FromBody] CreateExerciseGoalRequest request)
        {
            try
            {
                var goal = await goalRepo.CreateExerciseGoalAsync(
                    request.UserId,
                    request.Description,
                    request.Deadline,
                    request.ExerciseId,
                    request.TargetWeight,
                    request.TargetTime
                );

                return Ok(goal);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating goal: {ex.Message}");
            }
        }

        // GET api/goals/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserGoals(int userId)
        {
            try
            {
                var goals = await goalRepo.GetUserGoalsAsync(userId);
                var responses = new List<UserGoalResponse>();

                foreach (var goal in goals)
                {
                    int daysLeft = (int)(goal.DateCreated.AddDays(30) - DateTime.UtcNow).TotalDays;

                    WeightGoalData? weightData = null;
                    ExerciseGoalData? exerciseData = null;

                    if (goal.GoalType == "Weight Loss" || goal.GoalType == "Weight Gain")
                    {
                        var wg = await goalRepo.GetWeightGoalByGoalIdAsync(goal.GoalID);
                        if (wg != null)
                            weightData = new WeightGoalData(wg.WGoalID, wg.TargetBW, wg.StartBW);
                    }
                    else if (goal.GoalType == "Exercise")
                    {
                        var eg = await goalRepo.GetExerciseGoalByGoalIdAsync(goal.GoalID);
                        if (eg != null)
                            exerciseData = new ExerciseGoalData(eg.EGoalID, eg.ExerciseID, eg.Exercise?.ExerciseName ?? "", eg.TargetWeight, eg.TargetTime);
                    }

                    responses.Add(new UserGoalResponse(
                        goal.GoalID,
                        goal.UserID,
                        goal.GoalType,
                        goal.GoalDescription,
                        goal.DateCreated,
                        goal.IsCompleted,
                        daysLeft,
                        weightData,
                        exerciseData
                    ));
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving goals: {ex.Message}");
            }
        }

        // PATCH api/goals/{goalId}/complete
        [HttpPatch("{goalId}/complete")]
        public async Task<IActionResult> CompleteGoal(int goalId)
        {
            try
            {
                await goalRepo.CompleteGoalAsync(goalId);
                return Ok(new { message = "Goal marked as completed" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error completing goal: {ex.Message}");
            }
        }

        // DELETE api/goal/{goalId}
        // Remove a user goal and any related rows:
        [HttpDelete("{goalId}")]
        public async Task<IActionResult> DeleteGoal(int goalId)
        {
            try
            {
                await goalRepo.DeleteGoalAsync(goalId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting goal: {ex.Message}");
            }
        }

        private decimal CalculateCalorieDeficit(decimal targetWeight, decimal startWeight, DateTime deadline)
        {
            try
            {
                decimal weightDifference = Math.Abs(targetWeight - startWeight);
                int days = (int)(deadline - DateTime.UtcNow).TotalDays;
                
                if (days <= 0)
                    days = 30;

                decimal weeksRemaining = days / 7m;
                if (weeksRemaining <= 0)
                    weeksRemaining = 1;

                decimal weeklyRate = weightDifference / weeksRemaining;

                // 1kg = 7700 calories, so 0.5kg/week = 3850 cal/week deficit
                return weeklyRate * 7700 / 7;
            }
            catch
            {
                return 0;
            }
        }
    }
}
