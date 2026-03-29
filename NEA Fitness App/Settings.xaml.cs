using System.Net.Http.Json;
using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp;

public partial class Settings : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };
    private int currentUserId;
    private decimal currentCalculatedMaintenance;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Settings()
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
            await LoadUserData();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load user data: {ex.Message}", "OK");
        }
    }

    private async Task LoadUserData()
    {
        try
        {
            var user = await _httpClient.GetFromJsonAsync<RegisterRequest>($"api/user/{currentUserId}", JsonOpts);
            if (user != null)
            {
                UsernameLabel.Text = user.UserName;
                DobPicker.Date = user.UserDOB.ToDateTime(TimeOnly.MinValue);
                WeightEntry.Text = user.BodyWeight.ToString("F1");
                HeightEntry.Text = user.Height.ToString("F1");
                GenderPicker.SelectedItem = user.Gender;
                ActivityPicker.SelectedItem = user.ActivityLevel;
                
                currentCalculatedMaintenance = user.MaintenanceGoal;
                MaintenanceGoalLabel.Text = currentCalculatedMaintenance.ToString("F0");

                // Set picker constraints
                DobPicker.MaximumDate = DateTime.Today.AddYears(-18);
                DobPicker.MinimumDate = DateTime.Today.AddYears(-120);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
            throw;
        }
    }

    private void OnMetricsChanged(object sender, EventArgs e)
    {
        if (decimal.TryParse(WeightEntry.Text, out decimal weight) && decimal.TryParse(HeightEntry.Text, out decimal height) && height > 0)
        {
            DateTime dob = DobPicker.Date;
            DateTime today = DateTime.Today;

            int age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;

            if (age < 18)
            {
                MaintenanceGoalLabel.Text = "Must be 18+ to calculate.";
                currentCalculatedMaintenance = 0;
                return;
            }

            decimal bmr = (10m * weight) + (6.25m * height) - (5m * age);

            string gender = GenderPicker.SelectedItem as string ?? "Prefer not to say";
            if (gender == "Male") bmr += 5;
            else if (gender == "Female") bmr -= 161;

            string activity = ActivityPicker.SelectedItem?.ToString() ?? "Sedentary";
            decimal multiplier = activity switch
            {
                "BMR (Bedridden)" => 1.0m,
                "Sedentary" => 1.2m,
                "Lightly Active" => 1.375m,
                "Moderately Active" => 1.55m,
                "Very Active" => 1.725m,
                "Extremely Active" => 1.9m,
                _ => 1.2m
            };

            currentCalculatedMaintenance = bmr * multiplier;
            MaintenanceGoalLabel.Text = $"{currentCalculatedMaintenance:F0}";
        }
    }

    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        var errors = new List<string>();

        if (!decimal.TryParse(WeightEntry.Text, out decimal weight) || weight < 0.1m || weight > 999.9m)
            errors.Add("Weight must be between 0.1 and 999.9 kg.");

        if (!decimal.TryParse(HeightEntry.Text, out decimal height) || height < 0.1m || height > 300m)
            errors.Add("Height must be between 0.1 and 300 cm.");

        if (GenderPicker.SelectedItem == null)
            errors.Add("Please select a biological sex.");

        if (ActivityPicker.SelectedItem == null)
            errors.Add("Please select an activity level.");

        if (errors.Any())
        {
            await DisplayAlert("Validation Error", string.Join("\n", errors), "OK");
            return;
        }

        try
        {
            var updateRequest = new RegisterRequest
            {
                UserID = currentUserId,
                UserName = UsernameLabel.Text,
                UserDOB = DateOnly.FromDateTime(DobPicker.Date),
                BodyWeight = weight,
                Height = height,
                Gender = GenderPicker.SelectedItem as string ?? "Prefer not to say",
                ActivityLevel = ActivityPicker.SelectedItem?.ToString() ?? "Sedentary",
                MaintenanceGoal = (decimal)Math.Round(currentCalculatedMaintenance, 0)
            };

            var response = await _httpClient.PutAsJsonAsync($"api/user/{currentUserId}", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                // Update local preferences
                Preferences.Default.Set("LocalUserMaintenanceGoal", currentCalculatedMaintenance.ToString("F0"));
                await DisplayAlert("Success", "Profile updated successfully!", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Failed to update profile: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", ex.Message, "OK");
        }
    }
}