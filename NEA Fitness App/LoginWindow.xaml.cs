namespace NEAFitnessApp;

using System.Net.Http.Json;
using NEAFitnessApp.Models;

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
        var url = "https://localhost:7281/api/auth/login";

        try
        {
            var response = await client.PostAsJsonAsync(url, loginData);

            if (response.IsSuccessStatusCode)
            {
                // Use JsonElement to safely parse the response without needing a User class
                var root = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

                // Safely extract the maintenanceGoal if it exists in the JSON
                if (root.TryGetProperty("maintenanceGoal", out var goalProperty))
                {
                    string goalValue = goalProperty.ToString();
                    Preferences.Default.Set("LocalUserMaintenanceGoal", goalValue);
                }

                Preferences.Default.Set("UserName", UsernameEntry.Text);

                if (Application.Current != null)
                {
                    Application.Current.OpenWindow(new Window(new AppShell()));
                    if (this.Window != null) Application.Current.CloseWindow(this.Window);
                }
            }
            else
            {
                await DisplayAlert("Login failed", "Invalid credentials", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Login Error: " + ex.Message, "OK");
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