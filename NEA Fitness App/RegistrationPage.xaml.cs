namespace NEAFitnessApp;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage()
	{
		InitializeComponent();
	}
	private void OnMetricsChanged(object sender, EventArgs e)
	{
        // >0 check for weight and height
			// BMI Calculation
				// Result Output and Store Value
    }
    // OnFinalizeRegistrationClicked will send the registration data to the backend, and if successful, will navigate to the Home Page (AppShell)
		// RegisterRequest code
			// validate response, display alert and navigate to loginscreen or main page
}