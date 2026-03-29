using System.Net.Http.Json;
using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp;

public partial class CreateExerciseGoalPage : ContentPage
{
    private readonly int userId;
    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };
    private List<Exercise> userExercises = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CreateExerciseGoalPage(int userId)
    {
        InitializeComponent();
        this.userId = userId;
        DeadlinePicker.MinimumDate = DateTime.Today.AddDays(1);
        DeadlinePicker.MaximumDate = DateTime.Today.AddMonths(12);
        DeadlinePicker.Date = DateTime.Today.AddMonths(3);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserExercises();
    }

    private async Task LoadUserExercises()
    {
        try
        {
            userExercises = await _httpClient.GetFromJsonAsync<List<Exercise>>(
                $"api/workout/exercises/user/{userId}", JsonOpts) ?? new();

            var exerciseNames = userExercises.Select(e => e.ExerciseName).ToArray();
            ExercisePicker.ItemsSource = exerciseNames;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load exercises: {ex.Message}", "OK");
        }
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        var errors = new List<string>();

        if (ExercisePicker.SelectedIndex < 0)
            errors.Add("Please select an exercise.");

        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            errors.Add("Description is required.");

        var selectedExercise = ExercisePicker.SelectedIndex >= 0 ? userExercises[ExercisePicker.SelectedIndex] : null;
        
        if (selectedExercise == null)
        {
            errors.Add("Please select a valid exercise.");
        }
        else
        {
            decimal targetWeight = 0;
            int targetTime = 0;

            if (selectedExercise.ExerciseType == "Strength")
            {
                if (!decimal.TryParse(TargetWeightEntry.Text, out targetWeight) || targetWeight <= 0)
                    errors.Add("Target weight must be a valid positive number.");
            }
            else if (selectedExercise.ExerciseType == "Cardio")
            {
                if (!int.TryParse(TargetTimeEntry.Text, out targetTime) || targetTime <= 0)
                    errors.Add("Target time must be a valid positive number.");
            }

            if (errors.Any())
            {
                await DisplayAlert("Validation Error", string.Join("\n", errors), "OK");
                return;
            }

            try
            {
                var request = new
                {
                    UserId = userId,
                    Description = DescriptionEntry.Text,
                    Deadline = DeadlinePicker.Date,
                    ExerciseId = selectedExercise.ExerciseID,
                    TargetWeight = targetWeight,
                    TargetTime = targetTime
                };

                var response = await _httpClient.PostAsJsonAsync("api/goals/exercise", request);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Exercise goal created!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Failed to create goal: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Error", ex.Message, "OK");
            }
        }
    }
}