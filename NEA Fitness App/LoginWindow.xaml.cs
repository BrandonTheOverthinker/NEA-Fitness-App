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

            var loginData = new RegisterRequest
            {
                UserName = UsernameEntry.Text,
                Password = PasswordEntry.Text
            };

        using var client = new HttpClient();

        string url = "https://localhost:7281/api/auth/login";

        try
        {
            var response = await client.PostAsJsonAsync(url, loginData);
            if (response.IsSuccessStatusCode) // Only navigate to backend if credentials are correct
            {
                Application.Current.OpenWindow(new Window(new AppShell())); // Opens the Home Page
                if (this.Window != null)
                    Application.Current.CloseWindow(this.Window);
            }
            else // Backend denied login
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
        var registerData = new RegisterRequest
        {
            UserName = UsernameEntry.Text,
            Password = PasswordEntry.Text
        };

        using var client = new HttpClient();

        string url = "https://localhost:7281/api/auth/register";

        try
        {
            var response = await client.PostAsJsonAsync(url, registerData);
            if (response.IsSuccessStatusCode) // (if backend saved to DB)
            {
                await DisplayAlert("Success", "Account Created successfully", "OK");
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Registration failed", error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", "Is the backend running? " + ex.Message, "OK");
        }
    }
    
}

