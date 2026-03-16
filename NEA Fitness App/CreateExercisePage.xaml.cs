using System.Text;
using System.Text.Json;

namespace NEAFitnessApp;

public partial class CreateExercisePage : ContentPage
{
    private readonly int _userId;
    private const string BaseUrl = "https://localhost:7281/api/workout";

    // Page accepts the current userId so it can link the new exercise to their library:
    public CreateExercisePage(int userId)
    {
        InitializeComponent();
        _userId = userId;
    }

    // Update the hint label when the user changes the exercise type picker:
    private void OnExerciseTypeChanged(object sender, EventArgs e)
    {
        TypeHintLabel.IsVisible = true;
        TypeHintLabel.Text = ExerciseTypePicker.SelectedItem?.ToString() switch
        {
            "Strength" => "Strength exercises log: weight (kg) and reps per set.",
            "Cardio" => "Cardio exercises log: distance (metres) and time (seconds) per set.",
            _ => string.Empty
        };
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        // Guard to ensure a real user ID was passed in before writing to DB:
        if (_userId == 0)
        {
            ErrorLabel.Text = "User session not found. Please log in again.";
            ErrorLabel.IsVisible = true;
            return;
        }

        // Validation:
        if (string.IsNullOrWhiteSpace(ExerciseNameEntry.Text))
        {
            ErrorLabel.Text = "Please enter an exercise name.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (ExerciseTypePicker.SelectedIndex == -1)
        {
            ErrorLabel.Text = "Please select an exercise type.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var request = new
        {
            ExerciseName = ExerciseNameEntry.Text.Trim(),
            ExerciseType = ExerciseTypePicker.SelectedItem.ToString(),
            UserId = _userId
        };

        try
        {
            var client = new HttpClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BaseUrl}/exercises/create", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Saved", $"{request.ExerciseName} added to the exercise library.", "OK");
                await Navigation.PopAsync(); // Return to wherever called this page using stack
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ErrorLabel.Text = $"Error: {error}";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Could not connect to server: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }
}