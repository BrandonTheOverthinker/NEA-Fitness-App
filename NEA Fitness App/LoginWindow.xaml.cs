namespace NEAFitnessApp;

using System.Net.Http.Json;
using NEAFitnessApp.Models; // Access to RegisterRequest.cs

public partial class LoginWindow : ContentPage
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please enter username and password", "OK");
            return;
        }

        var loginData = new RegisterRequest { UserName = UsernameEntry.Text, Password = PasswordEntry.Text };

        using var client = new HttpClient();
        string url = "https://localhost:7281/api/auth/login";

        try
        {
            var response = await client.PostAsJsonAsync(url, loginData);

            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response the same way as RegistrationPage.xaml.cs:
                var root = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

                // Save the userID so all pages (food log, workout log etc.) can use it:
                if (root.TryGetProperty("userID", out var idProperty))
                    Preferences.Set("CurrentUserID", idProperty.GetInt32());

                // Save maintenance goal and username for the home page:
                if (root.TryGetProperty("maintenanceGoal", out var goalProperty))
                    Preferences.Default.Set("LocalUserMaintenanceGoal", goalProperty.ToString());

                Preferences.Default.Set("UserName", UsernameEntry.Text);

                // Open the main app shell and close the login window:
                if (Application.Current != null)
                {
                    Application.Current.OpenWindow(new Window(new AppShell()));
                    if (this.Window != null) Application.Current.CloseWindow(this.Window);
                }
            }
            else
            {
                await DisplayAlert("Login failed", "Invalid Username or Password.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", "Is the backend running? " + ex.Message, "OK");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.OpenWindow(new Window(new RegistrationPage()));
            if (this.Window != null)
                Application.Current.CloseWindow(this.Window);
        }
        else
        {
            await DisplayAlert("Error", "Application.Current is null. Unable to open a new window.", "OK");
        }
    }
}