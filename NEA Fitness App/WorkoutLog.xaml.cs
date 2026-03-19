using System.Text;
using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp;

public partial class WorkoutLog : ContentPage
{
    // I read the CurrentUserID from Preferences - set during login/registration:
    private int CurrentUserId => Preferences.Get("CurrentUserID", 0);
    private const string BaseUrl = "https://localhost:7281/api/workout";

    private List<Exercise> allExercises = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public WorkoutLog()
    {
        InitializeComponent();
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
            var client = new HttpClient();
            var response = await client.GetAsync($"{BaseUrl}/exercises/user/{CurrentUserId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                // Deserialise using Exercise from Models/Exercise.cs:
                allExercises = JsonSerializer.Deserialize<List<Exercise>>(json, JsonOpts) ?? new();
                ExerciseLibraryView.ItemsSource = allExercises;
            }
        }
        catch { /* Silently fail - page still usable */ }
    }

    // Live search filter on personal library:
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLower() ?? string.Empty;
        ExerciseLibraryView.ItemsSource = string.IsNullOrWhiteSpace(query)
            ? allExercises
            : allExercises.Where(ex => ex.ExerciseName.ToLower().Contains(query)).ToList();
    }

    private async void OnCreateExerciseClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateExercisePage(CurrentUserId));
    }

    private async void OnViewHistoryClicked(object sender, EventArgs e)
    {
        // I use Exercise from Models/Exercise.cs so no private DTO class is needed:
        if (sender is Button btn && btn.CommandParameter is Exercise exercise)
        {
            await Navigation.PushAsync(new ExerciseHistoryPage(CurrentUserId, exercise.ExerciseID, exercise.ExerciseName));
        }
    }

    private async void OnStartWorkoutClicked(object sender, EventArgs e)
    {
        StartErrorLabel.IsVisible = false;

        // Guard to ensure the user is logged in before trying to write to DB:
        if (CurrentUserId == 0)
        {
            StartErrorLabel.Text = "User session not found. Please log in again.";
            StartErrorLabel.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(WorkoutNameEntry.Text))
        {
            StartErrorLabel.Text = "Please enter a workout name.";
            StartErrorLabel.IsVisible = true;
            return;
        }

        var request = new
        {
            UserID = CurrentUserId,
            WorkoutName = WorkoutNameEntry.Text.Trim(),
            WorkoutNotes = WorkoutNotesEntry.Text?.Trim() ?? string.Empty
        };

        try
        {
            var client = new HttpClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/start", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();

                // Deserialise using the shared WorkoutSummary model:
                var workout = JsonSerializer.Deserialize<WorkoutSummary>(responseJson, JsonOpts);

                if (workout != null)
                {
                    // Navigate to the active session, passing the created workout's ID:
                    await Navigation.PushAsync(new ActiveWorkoutPage(
                        CurrentUserId,
                        workout.WorkoutID,
                        workout.WorkoutName));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                StartErrorLabel.Text = $"Could not start workout: {error}";
                StartErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            StartErrorLabel.Text = $"Connection error: {ex.Message}";
            StartErrorLabel.IsVisible = true;
        }
    }
}