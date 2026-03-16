using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEAFitnessApp;

public partial class WorkoutLog : ContentPage
{
    // TODO: replace with real session/auth system (same pattern as food log pages)
    private const int CurrentUserId = 1;
    private const string BaseUrl = "https://localhost:7281/api/workout";

    private List<ExerciseItem> _allExercises = new();

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
                _allExercises = JsonSerializer.Deserialize<List<ExerciseItem>>(json, JsonOpts) ?? new();
                ExerciseLibraryView.ItemsSource = _allExercises;
            }
        }
        catch { /* Silently fail - page still usable */ }
    }

    // Live search filter on personal library:
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLower() ?? string.Empty;
        ExerciseLibraryView.ItemsSource = string.IsNullOrWhiteSpace(query)
            ? _allExercises
            : _allExercises.Where(ex => ex.ExerciseName.ToLower().Contains(query)).ToList();
    }

    private async void OnCreateExerciseClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateExercisePage(CurrentUserId));
    }

    private async void OnViewHistoryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ExerciseItem exercise)
        {
            await Navigation.PushAsync(new ExerciseHistoryPage(CurrentUserId, exercise.ExerciseID, exercise.ExerciseName));
        }
    }

    private async void OnStartWorkoutClicked(object sender, EventArgs e)
    {
        StartErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(WorkoutNameEntry.Text))
        {
            StartErrorLabel.Text = "Please enter a workout name.";
            StartErrorLabel.IsVisible = true;
            return;
        }

        var request = new
        {
            UserId = CurrentUserId,
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
                var workout = JsonSerializer.Deserialize<WorkoutDto>(responseJson, JsonOpts);

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

    // Data Transfer Objects (DTOs) for API responses:
    private class ExerciseItem
    {
        [JsonPropertyName("exerciseID")]
        public int ExerciseID { get; set; }
        [JsonPropertyName("exerciseName")]
        public string ExerciseName { get; set; } = string.Empty;
        [JsonPropertyName("exerciseType")]
        public string ExerciseType { get; set; } = string.Empty;
    }

    private class WorkoutDto
    {
        [JsonPropertyName("workoutID")]
        public int WorkoutID { get; set; }
        [JsonPropertyName("workoutName")]
        public string WorkoutName { get; set; } = string.Empty;
    }
}