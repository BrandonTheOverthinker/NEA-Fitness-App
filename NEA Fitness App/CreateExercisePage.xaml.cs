using System.Text;
using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp;

public partial class CreateExercisePage : ContentPage
{
    private readonly int userId;
    private const string BaseUrl = "https://localhost:7281/api/workout";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Page accepts the current userId so it can link the new exercise to their library:
    public CreateExercisePage(int userId)
    {
        InitializeComponent();
        this.userId = userId;
    }

    // Update the hint label when the user changes the exercise type picker:
    private void OnExerciseTypeChanged(object sender, EventArgs e)
    {
        TypeHintLabel.IsVisible = true;
        TypeHintLabel.Text = ExerciseTypePicker.SelectedItem?.ToString() switch
        {
            "Strength" => "Strength exercises log: weight (kg) and reps per set.",
            "Cardio" => "Cardio exercises log: distance (metres) and time per set.",
            _ => string.Empty
        };
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        // Guard to ensure a real user ID was passed in before writing to DB:
        if (this.userId == 0)
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

        // Match the backend DTO property names exactly (ExerciseName, ExerciseType, UserId)
        var request = new
        {
            ExerciseName = ExerciseNameEntry.Text.Trim(),
            ExerciseType = ExerciseTypePicker.SelectedItem.ToString(),
            UserId = this.userId
        };

        try
        {
            var client = new HttpClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/exercises/create", content);

            var responseJson = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"POST {BaseUrl}/exercises/create -> {(int)response.StatusCode} {response.StatusCode}; body: '{responseJson}'");

            if (response.IsSuccessStatusCode)
            {
                Exercise? created = null;
                try
                {
                    created = JsonSerializer.Deserialize<Exercise>(responseJson, JsonOpts);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Deserialise created exercise failed: {ex.Message}");
                }

                await DisplayAlert("Saved", $"{created?.ExerciseName ?? request.ExerciseName} added to the exercise library.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                // Show status code and backend body for better debugging
                ErrorLabel.Text = $"Error ({(int)response.StatusCode}): {responseJson}";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Could not connect to server: {ex.Message}";
            ErrorLabel.IsVisible = true;
            System.Diagnostics.Debug.WriteLine($"Exception posting exercise: {ex}");
        }
    }
}