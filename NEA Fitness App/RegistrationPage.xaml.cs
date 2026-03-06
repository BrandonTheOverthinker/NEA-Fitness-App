using System;
using System.Globalization;
using System.Net.Http.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp
{
    public partial class RegistrationPage : ContentPage
    {
        private decimal currentCalculatedMaintenance; // Class-level variable to store the calculated value for later use

        public RegistrationPage()
        {
            InitializeComponent();

            DobPicker.MaximumDate = DateTime.Today.AddYears(-18);
            DobPicker.MinimumDate = DateTime.Today.AddYears(-120);
            
            // Remove any previous user data to ensure a clean registration process:
            Preferences.Default.Remove("LocalUserMaintenanceGoal");
            Preferences.Default.Remove("UserName");
        }

        private void OnMetricsChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(WeightEntry.Text, out decimal weight) && decimal.TryParse(HeightEntry.Text, out decimal height) && height > 0)
            {
                DateTime dob = DobPicker.Date;
                DateTime today = DateTime.Today;

                int age = today.Year - dob.Year;
                if (dob > today.AddYears(-age)) age--; // If birthday hasn't occurred this year, remove a year from age

                if (age < 18)
                {
                    BmrDisplay.Text = "Must be 18+ to calculate.";
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

                // Store the final maintenance calorie result in the class-level variable
                currentCalculatedMaintenance = bmr * multiplier;

                BmrDisplay.Text = $"{currentCalculatedMaintenance:F0}";
            }
        }

        private async void OnFinaliseRegistrationClicked(object sender, EventArgs e)
        {
            var registrationErrors = new List<string>();

            if (string.IsNullOrEmpty(UsernameEntry.Text) || UsernameEntry.Text.Length > 50)
                registrationErrors.Add("Username must be between 1 and 50 characters (inclusive).");
            if (string.IsNullOrEmpty(PasswordEntry.Text) || PasswordEntry.Text.Length < 8)
                registrationErrors.Add("Password must be at least 8 characters in length.");
            if (!decimal.TryParse(WeightEntry.Text, out decimal weightCheck) || weightCheck < 2.0m || weightCheck > 699.9m) // Max weight heavier than any adult human, min weight lighter
                registrationErrors.Add("Please enter a weight between 2.0Kg and 699.9Kg (inclusive).");
            if (!decimal.TryParse(HeightEntry.Text, out decimal heightCheck) || heightCheck < 20.0m || heightCheck > 299.9m) // Max height higher than any adult human, min height shorter
                registrationErrors.Add("Please enter a height between 20.0cm and 299.9cm (inclusive).");
            if (GenderPicker.SelectedItem == null)
                registrationErrors.Add("Please select your Biological Sex. This helps fine-tune your maintenance calorie calculation.");
            if (ActivityPicker.SelectedItem == null)
                registrationErrors.Add("Please select an Activity Level.");

            if (registrationErrors.Any())
            {
                await DisplayAlert("Registration Failed.", string.Join("\n", registrationErrors), "OK");
                return;
            }


            // Database Registration:
            var registrationData = new RegisterRequest
            {
                UserID = 0, // Created later by the backend, so set to 0 for now
                UserName = UsernameEntry.Text,
                Password = PasswordEntry.Text,
                UserDOB = DateOnly.FromDateTime(DobPicker.Date),
                BodyWeight = decimal.TryParse(WeightEntry.Text, out decimal weight) ? weight : 0,
                Height = decimal.TryParse(HeightEntry.Text, out decimal height) ? height : 0,
                Gender = GenderPicker.SelectedItem as string ?? "Prefer not to say",
                ActivityLevel = ActivityPicker.SelectedItem?.ToString() ?? "Sedentary",
                MaintenanceGoal = (decimal)Math.Round(currentCalculatedMaintenance, 0)
            };

            using var client = new HttpClient();
            var url = "https://localhost:7281/api/auth/register";
            try
            {
                // API Call to register the user with the calculated maintenance goal:
                var response = await client.PostAsJsonAsync(url, registrationData);

                if (response.IsSuccessStatusCode)
                {
                    var createdUser = await response.Content.ReadFromJsonAsync<RegisterRequest>();

                    if (createdUser != null)
                    {
                        // Save everything locally:
                        Preferences.Default.Set("LocalUserMaintenanceGoal", currentCalculatedMaintenance.ToString("F0"));
                        Preferences.Default.Set("UserName", UsernameEntry.Text);

                        Preferences.Set("CurrentUserID", createdUser.UserID); // Needed for FoodLog

                        await DisplayAlert("Saved", $"Goal: {currentCalculatedMaintenance:F0}", "OK");
                        Application.Current.MainPage = new AppShell();
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"API Error: {errorContent}", "OK");
                }
            }
            catch
            {
                // Don't have a real ID when testing so use -1
                Preferences.Set("CurrentUserID", -1);
                // Navigate anyway for testing purposes
                await DisplayAlert("Offline", "Goal saved locally.", "OK");
                Application.Current.MainPage = new AppShell();
            }
        }
    }
}