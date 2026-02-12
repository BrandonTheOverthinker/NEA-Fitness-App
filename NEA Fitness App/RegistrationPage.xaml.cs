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