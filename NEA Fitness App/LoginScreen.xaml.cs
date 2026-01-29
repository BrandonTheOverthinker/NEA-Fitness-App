namespace NEA_Fitness_App;

public partial class LoginScreen : ContentPage
{
	public LoginScreen()
	{
		InitializeComponent();
		Page = new ContentPage()
		{
			Content = new VerticalStackLayout
			{
				Children = {
					new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
					}
				}
			}
		};
	}

    private async void LoginButton_Pressed(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}