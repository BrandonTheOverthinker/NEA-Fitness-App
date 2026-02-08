namespace NEAFitnessApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new LoginWindow());
            // TODO: Ask user to create an account or log in
        }
    }
}