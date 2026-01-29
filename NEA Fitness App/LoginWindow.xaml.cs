namespace NEA_Fitness_App;

public partial class LoginWindow : ContentPage
{
	public LoginWindow()
	{
		InitializeComponent();
	}
    private void OnLoginClicked(object sender, EventArgs e)
    {
        // TODO: Connect to database and validate login credentials

        Application.Current.OpenWindow(new Window(new AppShell())); // Opens the Home Page

        var currentWindow = this.Window;
        if (currentWindow != null)
            Application.Current.CloseWindow(currentWindow);
    }
}