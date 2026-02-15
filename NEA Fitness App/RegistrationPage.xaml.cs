using System.Net.Http.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp
{
	public partial class RegistrationPage : ContentPage
	{
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
				if (dob > today.AddYears(-age)) // If birthday hasn't occurred this year
					age--;
				if (age < 18)
				{
                    BmrDisplay.Text = "Must be 18+ to calculate.";
                    return;
                }

				decimal bmr = (10m * weight) + (6.25m * height) - (5m * age); // Mifflin-St Jeor Equation

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
                    _ => 1.0m // Default fallback
                };

                decimal maintenanceCalories = bmr * multiplier;

                BmrDisplay.Text = $"{maintenanceCalories:F0}"; // Round to 0 dp since fractions of calories aren't meaningful
            }
		}

		private async void OnFinaliseRegistrationClicked(object sender, EventArgs e)
		{
			// OnFinaliseRegistrationClicked will send the registration data to the backend, and if successful, will navigate to the Home Page (AppShell)
			if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text) || !decimal.TryParse(WeightEntry.Text, out decimal w) || !decimal.TryParse(HeightEntry.Text, out decimal h))
			{
				await DisplayAlert("Error", "Please fill in all fields.", "OK");
				return;
			}
            string selectedGender = GenderPicker.SelectedItem?.ToString() ?? "Prefer not to say";
            string selectedActivity = ActivityPicker.SelectedItem?.ToString() ?? "Sedentary";
            var registerData = new RegisterRequest
			{
				UserName = UsernameEntry.Text,
				Password = PasswordEntry.Text,
				UserDOB = DateOnly.FromDateTime(DobPicker.Date),
				BodyWeight = w,
				Height = h,
				Gender = selectedGender,
				ActivityLevel = selectedActivity
            };

			using var client = new HttpClient();
			string url = "https://localhost:7281/api/auth/register";

			try
			{
				var response = client.PostAsJsonAsync(url, registerData).Result;
				if (response.IsSuccessStatusCode)
				{
					// This block works like cache for the rest of the app,
					// so that bmr can be remembered by the whole program without having to use the database:
                    Preferences.Default.Set("UserName", registerData.UserName);
                    Preferences.Default.Set("UserBMR", BmrDisplay.Text);

					await DisplayAlert("Welcome!", "Account Created Successfully.", "OK");
					Application.Current.MainPage = new AppShell();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Registration Failed", error, "OK");
                }
            }
			catch (Exception ex)
			{
                await DisplayAlert("Connection Error", "Is the backend running? " + ex.Message, "OK");
            }
		}
	}
}