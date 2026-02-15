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
            // Check if a calculation has actually happened:
            if (currentCalculatedMaintenance <= 0)
            {
                await DisplayAlert("Error", "Please enter valid metrics first.", "OK");
                return;
            }

            // Database Registration:
            var registrationData = new RegisterRequest
            {
                UserName = UsernameEntry.Text,
                Password = PasswordEntry.Text,
                UserDOB = DateOnly.FromDateTime(DobPicker.Date),
                BodyWeight = decimal.TryParse(WeightEntry.Text, out decimal weight) ? weight : 0,
                Height = decimal.TryParse(HeightEntry.Text, out decimal height) ? height : 0,
                Gender = GenderPicker.SelectedItem as string ?? "Prefer not to say",
                ActivityLevel = ActivityPicker.SelectedItem?.ToString() ?? "Sedentary",
                MaintenanceGoal = Math.Round(currentCalculatedMaintenance, 0)
            };

            Preferences.Default.Set("MaintenanceGoal", currentCalculatedMaintenance.ToString("F0"));
            Preferences.Default.Set("UserName", UsernameEntry.Text);

            using var client = new HttpClient();
            var url = "https://localhost:7281/api/auth/register";
            try
            {
                var response = await client.PostAsJsonAsync(url, registrationData);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Saved", $"Goal: {currentCalculatedMaintenance:F0}", "OK");
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    //await DisplayAlert("Error", "API Registration failed.", "OK");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"API Error: {errorContent}", "OK");
                }
            }
            catch
            {
                // Navigate anyway for testing purposes
                await DisplayAlert("Offline", "Goal saved locally.", "OK");
                Application.Current.MainPage = new AppShell();
            }
        }
    }
}