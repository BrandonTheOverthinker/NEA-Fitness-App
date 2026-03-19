using System.Text.Json;
using NEAFitnessApp.Models;

namespace NEAFitnessApp;

// View model for one row in the history table:
public class SessionHistoryRow
{
    public string DateStr { get; set; } = string.Empty; // e.g. "01 Mar"
    public string SetCount { get; set; } = string.Empty; // e.g. "4 sets"
    public string BestSet { get; set; } = string.Empty; // e.g. "100kg x 5" or "1000m / 240s"
    public string TotalVolume { get; set; } = string.Empty; // e.g. "1520" (strength only)
}

public partial class ExerciseHistoryPage : ContentPage
{
    private readonly int userId;
    private readonly int exerciseId;
    private const string BaseUrl = "https://localhost:7281/api/workout";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExerciseHistoryPage(int userId, int exerciseId, string exerciseName)
    {
        InitializeComponent();
        this.userId = userId;
        this.exerciseId = exerciseId;
        PageTitleLabel.Text = exerciseName;
        Title = exerciseName;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistory();
    }

    private async Task LoadHistory()
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{BaseUrl}/history/{this.userId}/{this.exerciseId}");
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();

            // Deserialise using ExerciseLogEntry from Models/ExerciseLogEntry.cs:
            var logs = JsonSerializer.Deserialize<List<ExerciseLogEntry>>(json, JsonOpts);
            if (logs == null || logs.Count == 0) return;

            // Fetch sets for each log entry and build the history table rows:
            var rows = new List<ExerciseHistoryEntry>();
            var chartData = new List<(string label, decimal value)>();

            foreach (var log in logs)
            {
                var setsResponse = await client.GetAsync($"{BaseUrl}/sets/{log.ExerciseLogID}");
                if (!setsResponse.IsSuccessStatusCode) continue;

                var setsJson = await setsResponse.Content.ReadAsStringAsync();

                // Deserialise using SetLog from Models/SetLog.cs:
                var sets = JsonSerializer.Deserialize<List<SetLog>>(setsJson, JsonOpts) ?? new();

                if (sets.Count == 0) continue;

                bool isStrength = sets[0].SetType == "Strength";

                // Read workout date from the nested WorkoutSummary property on ExerciseLogEntry:
                string dateStr = log.Workout?.WorkoutTime.ToString("dd MMM") ?? "?";

                string bestSet;
                string totalVolume = "-";
                decimal chartValue;

                if (isStrength)
                {
                    // Best set = highest weight lifted:
                    var best = sets.OrderByDescending(s => s.SetWeightKG).First();
                    bestSet = $"{best.SetWeightKG}kg x {best.Reps}";

                    // Total volume = sum of (weight x reps) across all sets:
                    decimal vol = sets.Sum(s => s.SetWeightKG * s.Reps);
                    totalVolume = vol.ToString("0");
                    chartValue = best.SetWeightKG; // Chart shows max weight trend
                }
                else
                {
                    // Best cardio set = furthest distance:
                    var best = sets.OrderByDescending(s => s.DistanceM).First();
                    bestSet = $"{best.DistanceM}m / {best.TimeSeconds}s";
                    chartValue = best.DistanceM;
                }

                rows.Add(new ExerciseHistoryEntry
                {
                    DateStr = dateStr,
                    SetCount = $"{sets.Count} set{(sets.Count != 1 ? "s" : "")}",
                    BestSet = bestSet,
                    TotalVolume = totalVolume,
                    ExerciseLogID = log.ExerciseLogID
                });

                chartData.Add((dateStr, chartValue));
            }

            HistoryTableView.ItemsSource = rows;
            BuildBarChart(chartData);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Build a simple bar chart using BoxViews proportional to the max value in the dataset.
    // For a production-quality chart, I could replace this with OxyPlot or Microcharts:
    private void BuildBarChart(List<(string label, decimal value)> data)
    {
        ChartBarsLayout.Children.Clear();
        if (data.Count == 0) return;

        decimal maxValue = data.Max(d => d.value);
        if (maxValue == 0) return;

        const double maxBarHeight = 140.0;

        foreach (var (label, value) in data)
        {
            double barHeight = (double)(value / maxValue) * maxBarHeight;

            var column = new VerticalStackLayout
            {
                Spacing = 4,
                WidthRequest = 40,
                VerticalOptions = LayoutOptions.End
            };

            // Value label above bar:
            column.Children.Add(new Label
            {
                Text = value.ToString("0"),
                FontSize = 9,
                HorizontalTextAlignment = TextAlignment.Center
            });

            // The bar itself:
            column.Children.Add(new BoxView
            {
                WidthRequest = 30,
                HeightRequest = Math.Max(barHeight, 4), // Minimum 4px so zero values are still visible
                HorizontalOptions = LayoutOptions.Center,
                CornerRadius = 3
            });

            // Date label below bar:
            column.Children.Add(new Label
            {
                Text = label,
                FontSize = 9,
                HorizontalTextAlignment = TextAlignment.Center
            });

            ChartBarsLayout.Children.Add(column);
        }
    }
}