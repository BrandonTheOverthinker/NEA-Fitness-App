namespace NEAFitnessApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for pages that are navigated to programmatically but are NOT flyout items (i.e. pushed onto the stack mid-session):
            Routing.RegisterRoute(nameof(CreateExercisePage), typeof(CreateExercisePage));
            Routing.RegisterRoute(nameof(ActiveWorkoutPage), typeof(ActiveWorkoutPage));
            Routing.RegisterRoute(nameof(ExerciseHistoryPage), typeof(ExerciseHistoryPage));
        }
    }
}
