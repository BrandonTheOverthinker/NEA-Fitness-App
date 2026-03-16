using Microsoft.Extensions.Logging;

namespace NEAFitnessApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register pages:
            builder.Services.AddTransient<FoodLog>();
            builder.Services.AddTransient<WorkoutLog>();
            builder.Services.AddTransient<CreateExercisePage>();
            builder.Services.AddTransient<ActiveWorkoutPage>();
            builder.Services.AddTransient<ExerciseHistoryPage>();


            return builder.Build();
        }
    }
}