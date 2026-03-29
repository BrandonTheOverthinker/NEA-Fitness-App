using System.Net.Http.Json;
using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp;

public partial class Goals : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };
    private int currentUserId;
    private decimal userCurrentWeight;
    private decimal userMaintenanceGoal;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Goals()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        currentUserId = Preferences.Get("CurrentUserID", 0);

        if (currentUserId == 0)
        {
            await DisplayAlert("Error", "User not found. Please log in again.", "OK");
            return;
        }

        try
        {
            await LoadUserAndGoals();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load goals: {ex.Message}", "OK");
        }
    }

    private async Task LoadUserAndGoals()
    {
        try
        {
            var user = await _httpClient.GetFromJsonAsync<RegisterRequest>($"api/user/{currentUserId}", JsonOpts);
            if (user != null)
            {
                userCurrentWeight = user.BodyWeight;
                userMaintenanceGoal = user.MaintenanceGoal;
            }

            try
            {
                var goals = await _httpClient.GetFromJsonAsync<List<UserGoalResponse>>($"api/goal/user/{currentUserId}", JsonOpts);
                if (goals != null)
                {
                    await PopulateGoalsUI(goals);
                }
                else
                {
                    await PopulateGoalsUI(new List<UserGoalResponse>());
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                System.Diagnostics.Debug.WriteLine($"Goals endpoint not found (404): {ex.Message}");
                await PopulateGoalsUI(new List<UserGoalResponse>());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading goals: {ex.Message}");
            throw;
        }
    }

    private async Task PopulateGoalsUI(List<UserGoalResponse> goals)
    {
        GoalsContainer.Clear();

        if (goals.Count == 0)
        {
            GoalsContainer.Add(new Label
            {
                Text = "No goals created yet. Tap 'Create New Goal' to get started!",
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(20)
            });
            return;
        }

        var weightGoals = goals.Where(g => g.GoalType == "Weight Loss" || g.GoalType == "Weight Gain").ToList();
        var exerciseGoals = goals.Where(g => g.GoalType == "Exercise").ToList();

        var activeWeight = weightGoals.Where(g => !g.IsCompleted).ToList();
        var completedWeight = weightGoals.Where(g => g.IsCompleted).ToList();

        var activeExercise = exerciseGoals.Where(g => !g.IsCompleted).ToList();
        var completedExercise = exerciseGoals.Where(g => g.IsCompleted).ToList();

        if (activeWeight.Any())
        {
            GoalsContainer.Add(CreateSectionHeader("Active Weight Goals"));
            foreach (var goal in activeWeight)
                GoalsContainer.Add(await CreateWeightGoalCard(goal));
        }

        if (activeExercise.Any())
        {
            GoalsContainer.Add(CreateSectionHeader("Active Exercise Goals"));
            foreach (var goal in activeExercise)
                GoalsContainer.Add(CreateExerciseGoalCard(goal));
        }

        // Completed history:
        if (completedWeight.Any() || completedExercise.Any())
        {
            GoalsContainer.Add(CreateSectionHeader("Completed Goals - History"));
            foreach (var goal in completedWeight)
                GoalsContainer.Add(await CreateWeightGoalCard(goal));
            foreach (var goal in completedExercise)
                GoalsContainer.Add(CreateExerciseGoalCard(goal));
        }
    }

    private Label CreateSectionHeader(string title)
    {
        return new Label
        {
            Text = title,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 20, 0, 10)
        };
    }

    private async Task<Border> CreateWeightGoalCard(UserGoalResponse goal)
    {
        var border = new Border
        {
            Padding = 15,
            Margin = new Thickness(0, 0, 0, 12),
            StrokeThickness = 1,
            Stroke = Colors.BlueViolet
        };

        decimal targetWeight = goal.WeightGoalData?.TargetBW ?? 0;
        decimal startWeight = goal.WeightGoalData?.StartBW ?? userCurrentWeight;
        decimal weeklyRate = goal.GoalType == "Weight Loss"
            ? (startWeight - targetWeight) / (goal.DaysUntilDeadline > 0 ? goal.DaysUntilDeadline / 7m : 1)
            : (targetWeight - startWeight) / (goal.DaysUntilDeadline > 0 ? goal.DaysUntilDeadline / 7m : 1);

        string warningMessage = "";
        if (goal.GoalType == "Weight Loss" && weeklyRate > 1m)
            warningMessage = " Warning: >1kg/week loss may be unhealthy";
        else if (goal.GoalType == "Weight Gain" && weeklyRate > 0.5m)
            warningMessage = " Warning: >0.5kg/week gain may be excessive";

        var stack = new VerticalStackLayout { Spacing = 8 };

        stack.Children.Add(new Label
        {
            Text = $"{goal.GoalDescription}",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold
        });

        stack.Children.Add(new Label
        {
            Text = $"Target: {targetWeight}kg | Current: {userCurrentWeight}kg",
            FontSize = 12,
            TextColor = Colors.Gray
        });

        stack.Children.Add(new Label
        {
            Text = $"Weekly Rate: {Math.Abs(weeklyRate):F2}kg/week{warningMessage}",
            FontSize = 12,
            TextColor = string.IsNullOrEmpty(warningMessage) ? Colors.Green : Colors.Orange
        });

        decimal progressPercentage = startWeight != targetWeight
            ? Math.Abs((userCurrentWeight - startWeight) / (targetWeight - startWeight))
            : 0;

        stack.Children.Add(new Label
        {
            Text = $"{progressPercentage:P0} Complete | {goal.DaysUntilDeadline} days left",
            FontSize = 11,
            TextColor = Colors.Gray
        });

        // Action buttons:
        var actions = new HorizontalStackLayout { Spacing = 8 };
        var deleteBtn = new Button { Text = "Remove", BackgroundColor = Colors.Transparent, TextColor = Colors.OrangeRed };
        deleteBtn.Clicked += async (_, __) => await DeleteGoalClicked(goal.GoalID);
        actions.Add(deleteBtn);

        if (goal.IsCompleted)
            actions.Add(new Label { Text = "Completed", TextColor = Colors.Green, FontSize = 12 }); // Show completed status

        stack.Children.Add(actions);

        border.Content = stack;
        return border;
    }

    private Border CreateExerciseGoalCard(UserGoalResponse goal)
    {
        var border = new Border
        {
            Padding = 15,
            Margin = new Thickness(0, 0, 0, 12),
            StrokeThickness = 1,
            Stroke = Colors.BlueViolet
        };

        var stack = new VerticalStackLayout { Spacing = 8 };

        stack.Children.Add(new Label
        {
            Text = goal.GoalDescription,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold
        });

        if (goal.ExerciseGoalData != null)
        {
            stack.Children.Add(new Label
            {
                Text = goal.ExerciseGoalData.ExerciseName,
                FontSize = 12,
                TextColor = Colors.Gray
            });

            string targetText = goal.ExerciseGoalData.ExerciseType == "Strength"
                ? $"Target: {goal.ExerciseGoalData.TargetWeight}kg"
                : $"Target: {goal.ExerciseGoalData.TargetDistance}m in {goal.ExerciseGoalData.TargetTime}s";

            stack.Children.Add(new Label
            {
                Text = targetText,
                FontSize = 12,
                TextColor = Colors.Gray
            });
        }

        stack.Children.Add(new Label
        {
            Text = $"Status: {(goal.IsCompleted ? "Completed" : $"In Progress - {goal.DaysUntilDeadline} days left")}",
            FontSize = 12,
            TextColor = goal.IsCompleted ? Colors.Green : Colors.Orange
        });

        var actions = new HorizontalStackLayout { Spacing = 8 };
        var deleteBtn = new Button { Text = "Remove", BackgroundColor = Colors.Transparent, TextColor = Colors.OrangeRed };
        deleteBtn.Clicked += async (_, __) => await DeleteGoalClicked(goal.GoalID);
        actions.Add(deleteBtn);

        stack.Children.Add(actions);

        border.Content = stack;
        return border;
    }

    private async Task DeleteGoalClicked(int goalId)
    {
        bool confirm = await DisplayAlert("Remove Goal", "Are you sure you want to remove this goal? This will delete associated goal data.", "Remove", "Cancel");
        if (!confirm) return;

        var response = await _httpClient.DeleteAsync($"api/goal/{goalId}");
        if (response.IsSuccessStatusCode)
        {
            await LoadUserAndGoals(); // refresh UI
        }
        else
        {
            var err = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Error", "Failed to remove goal: " + err, "OK");
        }
    }

    private async void OnCreateGoalClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet(
            "Create Goal",
            "Cancel",
            null,
            "Weight Loss Goal",
            "Weight Gain Goal",
            "Exercise Goal"
        );

        if (action == "Weight Loss Goal")
            await Navigation.PushAsync(new CreateWeightGoalPage(currentUserId, "Weight Loss"));
        else if (action == "Weight Gain Goal")
            await Navigation.PushAsync(new CreateWeightGoalPage(currentUserId, "Weight Gain"));
        else if (action == "Exercise Goal")
            await Navigation.PushAsync(new CreateExerciseGoalPage(currentUserId));
    }
}