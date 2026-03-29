using System.Net.Http.Json;
using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };
        private int currentUserId;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            currentUserId = Preferences.Get("CurrentUserID", 0);

            string storedGoal = Preferences.Default.Get("LocalUserMaintenanceGoal", "Not Set");

            if (storedGoal != "Not Set" && decimal.TryParse(storedGoal, out decimal goalValue))
            {
                LocalUserMaintenanceGoal.Text = goalValue.ToString("F0");

                try
                {
                    await LoadTodaysSummary();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading summary: {ex.Message}");
                }
            }
            else
            {
                LocalUserMaintenanceGoal.Text = "Not Set";
            }

            XpDisplay.Text = "0";
        }

        private async Task LoadTodaysSummary()
        {
            try
            {
                var summaryResponse = await _httpClient.GetFromJsonAsync<JsonElement>(
                    $"api/analytics/user/{currentUserId}/summary", JsonOpts);

                if (summaryResponse.ValueKind != JsonValueKind.Null)
                {
                    // Get today's calories
                    decimal todayCalories = (decimal)summaryResponse.GetProperty("todayCalories").GetDouble();
                    CurrentCaloriesLabel.Text = todayCalories.ToString("F0");


                    // Get weekly workouts
                    int weeklyWorkouts = summaryResponse.GetProperty("weeklyWorkouts").GetInt32();
                    int weeklyDuration = summaryResponse.GetProperty("weeklyDuration").GetInt32();
                    decimal weeklyCalories = (decimal)summaryResponse.GetProperty("weeklyCalories").GetDouble();

                    WeeklySummaryLabel.Text = $"Workouts: {weeklyWorkouts}\n" +
                                             $"Duration: {weeklyDuration}min\n" +
                                             $"Calories: {weeklyCalories:F0}kcal";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadTodaysSummary: {ex.Message}");
                WeeklySummaryLabel.Text = "Error loading data";
            }
        }
    }
}
