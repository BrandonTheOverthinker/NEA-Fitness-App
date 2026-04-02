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
    
        public GoalController(IGoalRepository goalRepo, IUserRepository userRepo)
        {
            this.goalRepo = goalRepo;
            this.userRepo = userRepo;
        }
        public record CreateWeightGoalRequest(int userId, string description, DateTime deadline, decimal targetWeight, decimal startWeight);
        public record CreateExerciseGoalRequest(int userId, string description, DateTime deadline, int exerciseId, decimal targetWeight,
            int targetTime);
        public record UserGoalResponse(int goalId, int userId, string goalType, string goalDescription, DateTime dateCreated,
            bool completed, int daysUntilDeadline, WeightGoalData? weightGoalData, ExerciseGoalData? exerciseGoalData);
        public record WeightGoalData(int wGoalID, decimal targetBW, decimal startBW);
        public record ExerciseGoalData(int eGoalID, int exerciseID, string exerciseName, decimal targetWeight, int targetTime);

        [HttpPost("weight")]
        public async Task<IActionResult> CreateWeightGoal([FromBody] CreateWeightGoalRequest request)
        {
            try 
            {
                if (request.targetWeight <= 0 || request.startWeight <= 0)
                    return BadRequest("Target and start weights must be positive.");

                var goal = await goalRepo.CreateWeightGoal(request.userId, request.description, request.deadline, request.targetWeight, request.startWeight);

                try // I made it so the weight goal is still created if the nutrition goal creation errors.
                {
                    decimal calorieDeficit = CalculateCalorieDeficit(request.targetWeight, request.startWeight, request.deadline);
                    var user = await userRepo.GetUserById(request.userId);
                    if (user != null)
                    {
                        decimal maintenanceGoal = user.MaintenanceGoal;
                        int targetCalories = Math.Max(1200, (int)(maintenanceGoal - Math.Abs(calorieDeficit)));

                        var weightGoal = await goalRepo.GetWeightGoalByGoalId(goal.GoalID);
                        if (weightGoal != null)
                        {
                            try
                            {


                                await goalRepo.CreateNutritionGoal(
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
                                Debug.WriteLine($"Failed to save NutritionGoal for WGoalID={weightGoal.WGoalID}: {exInner}");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"User not found when creating nutrition goal for UserId={request.userId}");
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

        [HttpPost("exercise")]
        public async Task<IActionResult> CreateExerciseGoal([FromBody] CreateExerciseGoalRequest request)
        {
            try
            {
                var goal = await goalRepo.CreateExerciseGoal(
                    request.userId,
                    request.description,
                    request.deadline,
                    request.exerciseId,
                    request.targetWeight,
                    request.targetTime);

                return Ok(goal);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating goal: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserGoals(int userId)
        {
            try
            {
                var goals = await goalRepo.GetUserGoals(userId);
                var responses = new List<UserGoalResponse>();

                foreach (var goal in goals)
                {
                    int daysLeft = (int)(goal.DateCreated.AddDays(30) - DateTime.UtcNow).TotalDays;

                    WeightGoalData? weightData = null;
                    ExerciseGoalData? exerciseData = null;

                    if (goal.GoalType == "Weight Loss" || goal.GoalType == "Weight Gain")
                    {
                        var wg = await goalRepo.GetWeightGoalByGoalId(goal.GoalID);
                        if (wg != null)
                        {
                            weightData = new WeightGoalData(wg.WGoalID, wg.TargetBW, wg.StartBW);
                        }
                    }
                    else if (goal.GoalType == "Exercise")
                    {

                        var eg = await goalRepo.GetExerciseGoalByGoalId(goal.GoalID);
                        if (eg != null)
                        {
                            exerciseData = new ExerciseGoalData(eg.EGoalID, eg.ExerciseID, eg.Exercise?.ExerciseName ?? "", eg.TargetWeight, eg.TargetTime);
                        }
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

        [HttpPatch("{goalId}/complete")]
        public async Task<IActionResult> CompleteGoal(int goalId)
        {
            try
            {
                await goalRepo.CompleteGoal(goalId);
                return Ok(new { message = "Goal marked as completed" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error completing goal: {ex.Message}");
            }
        }

        [HttpDelete("{goalId}")]
        public async Task<IActionResult> DeleteGoal(int goalId)
        {
            try
            {
                await goalRepo.DeleteGoal(goalId);
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
                {
                    days = 30;
                }

                decimal weeksRemaining = days / 7m;

                if (weeksRemaining <= 0)
                {
                    weeksRemaining = 1;
                }

                decimal weeklyRate = weightDifference / weeksRemaining;
                return weeklyRate * 7700 / 7;


            }
            catch
            {
                return 0;
            }
        }
    }
}
