using System.Net.Http.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp
{
	public partial class RegistrationPage : ContentPage
	{
		public RegistrationPage() => InitializeComponent();

		private void OnMetricsChanged(object sender, TextChangedEventArgs e)
		{
			// >0 check for weight and height
			// BMI Calculation
			// Result Output and Store Value
			if (decimal.TryParse(WeightEntry.Text, out decimal weight) && decimal.TryParse(HeightEntry.Text, out decimal height) && height > 0)
			{
				DobPicker.MaximumDate = DateTime.Today.AddYears(-18);
				DobPicker.MinimumDate = DateTime.Today.AddYears(-120);
				DateTime dob = DobPicker.Date;
				DateTime today = DateTime.Today;

				int age = today.Year - dob.Year;
				if (dob > today.AddYears(-age)) // if birthday hasn't occurred this year
					age--;
				if (age < 18)
				{
					// error
				}
				
			}
		}

		private void OnFinaliseRegistrationClicked(object sender, EventArgs e)
		{
			// OnFinaliseRegistrationClicked will send the registration data to the backend, and if successful, will navigate to the Home Page (AppShell)
			// RegisterRequest code
			// validate response, display alert and navigate to loginscreen or main page
		}
	}
}