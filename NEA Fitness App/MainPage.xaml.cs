namespace NEAFitnessApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            string storedGoal = Preferences.Default.Get("LocalUserMaintenanceGoal", "Not Set");

            if (storedGoal != "Not Set" && decimal.TryParse(storedGoal, out decimal goalValue))
            {
                LocalUserMaintenanceGoal.Text = goalValue.ToString("F0");

                decimal current = 0m;
                CurrentCaloriesLabel.Text = current.ToString("F0");

                if (goalValue > 0)
                {
                    CalorieProgressBar.Progress = (double)(current / goalValue);
                }
            }
            else
            {
                LocalUserMaintenanceGoal.Text = "Not Set";
            }

            XpDisplay.Text = "0";
        }
    }
}
