using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEAFitnessApp;

public class WorkoutExerciseViewModel
{
    public int ExerciseLogId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty; // "Strength" or "Cardio"
    public List<string> SetSummaries { get; set; } = new(); // e.g. ["Set 1: 80kg x 8", "Set 2: 80kg x 6"]

    // Displayed as subtitle in the list item:
    public string SetSummary => SetSummaries.Count == 0
        ? "No sets logged yet — tap to add"
        : string.Join("  |  ", SetSummaries);
}

public partial class ActiveWorkoutPage : ContentPage
{
    private readonly int _userId;
    private readonly int _workoutId;
    private readonly string _workoutName;
    private const string BaseUrl = "https://localhost:7281/api/workout";

    private IDispatcherTimer? _timer;
    private int _elapsedSeconds = 0;

    private readonly List<WorkoutExerciseViewModel> _exercises = new();
    private WorkoutExerciseViewModel? _selectedExercise;

    // JSON handles polymorphic Exercise responses from the API:
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ActiveWorkoutPage(int userId, int workoutId, string workoutName)
    {
        InitializeComponent();
        _userId = userId;
        _workoutId = workoutId;
        _workoutName = workoutName;

        WorkoutNameLabel.Text = workoutName;
        ExercisesCollectionView.ItemsSource = _exercises;

        StartTimer();
    }

    private void StartTimer()
    {
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _elapsedSeconds++;
        // Format as MM:SS (switches to HH:MM:SS for long sessions):
        TimerLabel.Text = _elapsedSeconds < 3600
            ? TimeSpan.FromSeconds(_elapsedSeconds).ToString(@"mm\:ss")
            : TimeSpan.FromSeconds(_elapsedSeconds).ToString(@"hh\:mm\:ss");
    }

    private async void OnAddExerciseClicked(object sender, EventArgs e)
    {
        // Give the user two options - pick from their library, or create a new exercise:
        string action = await DisplayActionSheet(
            "Add Exercise",
            "Cancel",
            null,
            "Choose from my library",
            "Create new exercise");

        if (action == "Choose from my library")
            await PickFromLibrary();
        else if (action == "Create new exercise")
        {
            // Push the create page, then open the library picker when it returns automatically:
            await Navigation.PushAsync(new CreateExercisePage(_userId));
            await PickFromLibrary();
        }
    }

    private async Task PickFromLibrary()
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{BaseUrl}/exercises/user/{_userId}");

            // Show a clear error if the request fails rather than silently returning:
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Could not load exercises: {error}", "OK");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var exercises = JsonSerializer.Deserialize<List<ExerciseDto>>(json, JsonOpts);

            if (exercises == null || exercises.Count == 0)
            {
                await DisplayAlert("No Exercises", "Your library is empty. Create an exercise first.", "OK");
                return;
            }

            var names = exercises.Select(e => e.ExerciseName).ToArray();
            string? picked = await DisplayActionSheet("Choose Exercise", "Cancel", null, names);

            if (picked == null || picked == "Cancel") return;

            var chosen = exercises.First(e => e.ExerciseName == picked);
            await AddExerciseToWorkout(chosen);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task AddExerciseToWorkout(ExerciseDto exercise)
    {
        var request = new
        {
            WorkoutId = _workoutId,
            UserId = _userId,
            ExerciseId = exercise.ExerciseId,
            ExerciseNotes = string.Empty
        };

        try
        {
            var client = new HttpClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/log-exercise", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var log = JsonSerializer.Deserialize<ExerciseLogDto>(responseJson, JsonOpts);

                if (log != null)
                {
                    var vm = new WorkoutExerciseViewModel
                    {
                        ExerciseLogId = log.ExerciseLogId,
                        ExerciseId = exercise.ExerciseId,
                        ExerciseName = exercise.ExerciseName,
                        ExerciseType = exercise.ExerciseType
                    };
                    _exercises.Add(vm);
                    // Refresh CollectionView:
                    ExercisesCollectionView.ItemsSource = null;
                    ExercisesCollectionView.ItemsSource = _exercises;
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Could not add exercise to workout: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Set Logger:
    private void OnExerciseSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not WorkoutExerciseViewModel selected) return;

        _selectedExercise = selected;
        SelectedExerciseLabel.Text = $"Logging set for: {selected.ExerciseName}";
        SetLoggerFrame.IsVisible = true;
        SetErrorLabel.IsVisible = false;

        // Polymorphic UI that shows the correct fields based on exercise type:
        StrengthFields.IsVisible = selected.ExerciseType == "Strength";
        CardioFields.IsVisible = selected.ExerciseType == "Cardio";

        // Clear previous entries:
        WeightEntry.Text = RepsEntry.Text = DistanceEntry.Text = TimeEntry.Text = RpeEntry.Text = string.Empty;
    }

    private async void OnLogSetClicked(object sender, EventArgs e)
    {
        if (_selectedExercise == null) return;
        SetErrorLabel.IsVisible = false;

        int setNumber = _selectedExercise.SetSummaries.Count + 1;
        bool isStrength = _selectedExercise.ExerciseType == "Strength";

        // Parse and validate inputs based on exercise type:
        decimal weight = 0; int reps = 0, distance = 0, time = 0;
        decimal rpe = 0;

        if (isStrength)
        {
            if (!decimal.TryParse(WeightEntry.Text, out weight) || weight < 0)
            { ShowSetError("Please enter a valid weight."); return; }
            if (!int.TryParse(RepsEntry.Text, out reps) || reps <= 0)
            { ShowSetError("Please enter a valid rep count."); return; }
        }
        else
        {
            if (!int.TryParse(DistanceEntry.Text, out distance) || distance <= 0)
            { ShowSetError("Please enter a valid distance."); return; }
            if (!int.TryParse(TimeEntry.Text, out time) || time <= 0)
            { ShowSetError("Please enter a valid time."); return; }
        }

        decimal.TryParse(RpeEntry.Text, out rpe); // RPE is optional

        var request = new
        {
            ExerciseLogId = _selectedExercise.ExerciseLogId,
            SetNumber = setNumber,
            SetType = _selectedExercise.ExerciseType,
            Reps = reps,
            SetWeightKG = weight,
            DistanceM = distance,
            TimeSeconds = time,
            RPE = rpe
        };

        try
        {
            var client = new HttpClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/log-set", content);

            if (response.IsSuccessStatusCode)
            {
                // Add a summary string to the exercise's set list:
                string summary = isStrength
                    ? $"Set {setNumber}: {weight}kg × {reps}"
                    : $"Set {setNumber}: {distance}m in {time}s";

                _selectedExercise.SetSummaries.Add(summary);

                // Refresh the CollectionView item to show updated set count:
                ExercisesCollectionView.ItemsSource = null;
                ExercisesCollectionView.ItemsSource = _exercises;

                // Clear inputs ready for next set:
                WeightEntry.Text = RepsEntry.Text = DistanceEntry.Text =
                    TimeEntry.Text = RpeEntry.Text = string.Empty;
            }
            else
            {
                ShowSetError("Failed to save set. Try again.");
            }
        }
        catch (Exception ex)
        {
            ShowSetError($"Error: {ex.Message}");
        }
    }

    private void ShowSetError(string message)
    {
        SetErrorLabel.Text = message;
        SetErrorLabel.IsVisible = true;
    }

    private async void OnFinishWorkoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Finish Workout",
            $"End workout? Duration: {TimerLabel.Text}",
            "Yes, finish",
            "Keep going");

        if (!confirm) return;

        _timer?.Stop();

        try
        {
            var client = new HttpClient();
            var request = new { DurationSeconds = _elapsedSeconds };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PatchAsync($"{BaseUrl}/{_workoutId}/finish", content);
        }
        catch { /* Non-critical - timer saved best effort */ }

        // Navigate back to WorkoutLog, clearing the active workout from the nav stack
        await Shell.Current.GoToAsync("//WorkoutLog");
    }

    // Data Transfer Objects (Dtos) for API responses:

    private class ExerciseDto
    {
        [JsonPropertyName("exerciseID")]
        public int ExerciseId { get; set; }
        [JsonPropertyName("exerciseName")]
        public string ExerciseName { get; set; } = string.Empty;
        [JsonPropertyName("exerciseType")]
        public string ExerciseType { get; set; } = string.Empty;
    }

    private class ExerciseLogDto
    {
        [JsonPropertyName("exerciseLogID")]
        public int ExerciseLogId { get; set; }
    }
}