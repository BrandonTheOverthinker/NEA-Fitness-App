using System.Net.Http.Json;
using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp;

public partial class Analytics : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };
    private int currentUserId;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Analytics()
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
            await LoadAnalyticsData();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load analytics: {ex.Message}", "OK");
        }
    }

    private async Task LoadAnalyticsData()
    {
        try
        {
            var summaryResponse = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"api/analytics/user/{currentUserId}/summary", JsonOpts);

            if (summaryResponse.ValueKind != JsonValueKind.Null)
            {
                int weeklyWorkouts = summaryResponse.GetProperty("weeklyWorkouts").GetInt32();
                int weeklyDuration = summaryResponse.GetProperty("weeklyDuration").GetInt32();

                WorkoutStatsLabel.Text = $"Workouts This Week: {weeklyWorkouts}\nTotal Duration: {weeklyDuration}min";

                var highestFood = summaryResponse.GetProperty("highestFood");
                var recentFood = summaryResponse.GetProperty("recentFood");
                
                string highestFoodName = "N/A";
                int highestFoodCals = 0;
                if (highestFood.GetProperty("foodName").ValueKind != JsonValueKind.Null)
                {
                    highestFoodName = highestFood.GetProperty("foodName").GetString() ?? "N/A";
                    highestFoodCals = highestFood.GetProperty("calories").ValueKind != JsonValueKind.Null 
                        ? highestFood.GetProperty("calories").GetInt32() 
                        : 0;
                }

                string recentFoodName = "N/A";
                int recentFoodCals = 0;
                if (recentFood.GetProperty("foodName").ValueKind != JsonValueKind.Null)
                {
                    recentFoodName = recentFood.GetProperty("foodName").GetString() ?? "N/A";
                    recentFoodCals = recentFood.GetProperty("calories").ValueKind != JsonValueKind.Null 
                        ? recentFood.GetProperty("calories").GetInt32() 
                        : 0;
                }

                NutritionInfoLabel.Text = $"Highest Calorie: {highestFoodName} ({highestFoodCals}kcal)\nMost Recent: {recentFoodName} ({recentFoodCals}kcal)";

                var goalProgress = summaryResponse.GetProperty("goalProgress");
                int weightGoals = goalProgress.GetProperty("weightGoalCount").GetInt32();
                decimal weightProgress = (decimal)goalProgress.GetProperty("weightGoalProgress").GetDouble();
                int exerciseGoals = goalProgress.GetProperty("exerciseGoalCount").GetInt32();

                GoalProgressLabel.Text = $"Weight Goal: {weightProgress:F0}% complete\nExercise Goals: {exerciseGoals} active";

                await LoadMacroAnalytics();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading analytics: {ex.Message}");
            throw;
        }
    }

    private async Task LoadMacroAnalytics()
    {
        try
        {
            var macroResponse = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"api/analytics/user/{currentUserId}/macros", JsonOpts);

            if (macroResponse.ValueKind != JsonValueKind.Null)
            {
                var today = macroResponse.GetProperty("today");
                var avg = macroResponse.GetProperty("sevenDayAverage");

                decimal todayProteins = (decimal)today.GetProperty("protein").GetDouble();
                decimal todayFat = (decimal)today.GetProperty("fat").GetDouble();
                decimal todayCarbs = (decimal)today.GetProperty("carbohydrates").GetDouble();
                decimal avgProtein = (decimal)avg.GetProperty("protein").GetDouble();
                decimal avgFat = (decimal)avg.GetProperty("fat").GetDouble();
                decimal avgCarbs = (decimal)avg.GetProperty("carbs").GetDouble();

                MacroAnalyticsLabel.Text = $"Today: P{todayProteins:F1}g | F{todayFat:F1}g | C{todayCarbs:F1}g\n" +
                                          $"7-Day Avg: P{avgProtein:F1}g | F{avgFat:F1}g | C{avgCarbs:F1}g";

                await BuildMacroSelector();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading macro analytics: {ex.Message}");
            throw;
        }
    }

    private async Task BuildMacroSelector()
    {
        MacroSelectorLayout.Children.Clear();

        var macros = new[] { "Protein", "Fat", "Carbs", "Calories", "Fibre" };

        foreach (var macro in macros)
        {
            var button = new Button
            {
                Text = macro,
                BackgroundColor = Color.FromRgb(81, 43, 212),
                TextColor = Colors.White,
                FontSize = 12,
                Padding = new Thickness(10, 5),
                Margin = new Thickness(2)
            };

            button.Clicked += async (s, e) => await OnMacroSelected(macro);
            MacroSelectorLayout.Children.Add(button);
        }
    }

    private async Task OnMacroSelected(string macroType)
    {
        int userId = Preferences.Get("CurrentUserID", 0);
        if (userId <= 0) return;

        try
        {
            var macroResponse = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"api/analytics/user/{userId}/macros", JsonOpts);

            if (macroResponse.ValueKind != JsonValueKind.Null)
            {
                var avg = macroResponse.GetProperty("sevenDayAverage");
                
                string value = macroType switch
                {
                    "Protein" => avg.GetProperty("protein").GetDouble().ToString("F1") + "g",
                    "Fat" => avg.GetProperty("fat").GetDouble().ToString("F1") + "g",
                    "Carbs" => avg.GetProperty("carbs").GetDouble().ToString("F1") + "g",
                    "Calories" => avg.GetProperty("calories").GetDouble().ToString("F0") + "kcal",
                    "Fibre" => avg.GetProperty("fibre").GetDouble().ToString("F1") + "g",
                    _ => "N/A"
                };

                await DisplayAlert(
                    $"{macroType} - 7 Day Average",
                    $"Average per day: {value}\n\n(Based on logged food data)",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load {macroType} data: {ex.Message}", "OK");
        }
    }

    private async void OnViewExerciseHistoryClicked(object sender, EventArgs e)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"https://localhost:7281/api/workout/exercises/user/{currentUserId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var exercises = JsonSerializer.Deserialize<List<Exercise>>(json, JsonOpts) ?? new();

                if (exercises.Count == 0)
                {
                    await DisplayAlert("No Exercises", "You haven't logged any exercises yet.", "OK");
                    return;
                }

                var names = exercises.Select(e => e.ExerciseName).ToArray();
                string? picked = await DisplayActionSheet("Choose Exercise", "Cancel", null, names);

                if (picked == null || picked == "Cancel") return;

                var chosen = exercises.First(e => e.ExerciseName == picked);
                await Navigation.PushAsync(new ExerciseHistoryPage(currentUserId, chosen.ExerciseID, chosen.ExerciseName));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}