using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEAFitnessApp;

// View model for one row in the history table
public class SessionHistoryRow
{
    public string DateStr { get; set; } = string.Empty; // e.g. "01 Mar"
    public string SetCount { get; set; } = string.Empty; // e.g. "4 sets"
    public string BestSet { get; set; } = string.Empty; // e.g. "100kg × 5"  or  "1000m / 240s"
    public string TotalVolume { get; set; } = string.Empty; // e.g. "1520" (only for strength)
}

public partial class ExerciseHistoryPage : ContentPage
{
    private readonly int _userId;
    private readonly int _exerciseId;
    private const string BaseUrl = "https://localhost:7281/api/workout";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExerciseHistoryPage(int userId, int exerciseId, string exerciseName)
    {
        InitializeComponent();
        _userId = userId;
        _exerciseId = exerciseId;
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
            var response = await client.GetAsync($"{BaseUrl}/history/{_userId}/{_exerciseId}");
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            var logs = JsonSerializer.Deserialize<List<ExerciseLogDto>>(json, JsonOpts);
            if (logs == null || logs.Count == 0) return;

            // Fetch sets for each log
            var rows = new List<SessionHistoryRow>();
            var chartData = new List<(string label, decimal value)>();

            foreach (var log in logs)
            {
                var setsResponse = await client.GetAsync($"{BaseUrl}/sets/{log.ExerciseLogId}");
                if (!setsResponse.IsSuccessStatusCode) continue;

                var setsJson = await setsResponse.Content.ReadAsStringAsync();
                var sets = JsonSerializer.Deserialize<List<SetDto>>(setsJson, JsonOpts) ?? new();

                if (sets.Count == 0) continue;

                bool isStrength = sets[0].SetType == "Strength";
                string dateStr = log.Workout?.WorkoutTime.ToString("dd MMM") ?? "?";

                string bestSet;
                string totalVolume = "-";
                decimal chartValue;

                if (isStrength)
                {
                    // Best set = highest weight lifted
                    var best = sets.OrderByDescending(s => s.SetWeightKG).First();
                    bestSet = $"{best.SetWeightKG}kg × {best.Reps}";

                    // Total volume = sum of (weight × reps) across all sets
                    decimal vol = sets.Sum(s => s.SetWeightKG * s.Reps);
                    totalVolume = vol.ToString("0");
                    chartValue = best.SetWeightKG; // Chart shows max weight trend
                }
                else
                {
                    // Best cardio set = furthest distance
                    var best = sets.OrderByDescending(s => s.DistanceM).First();
                    bestSet = $"{best.DistanceM}m / {best.TimeSeconds}s";
                    chartValue = best.DistanceM;
                }

                rows.Add(new SessionHistoryRow
                {
                    DateStr = dateStr,
                    SetCount = $"{sets.Count} set{(sets.Count != 1 ? "s" : "")}",
                    BestSet = bestSet,
                    TotalVolume = totalVolume
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

    // Build simple horizontal bar chart using BoxViews;
    // Each bar's height is proportional to the max value in the dataset;
    // For a production-quality chart, could replace this with OxyPlot or Microcharts.
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
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center
            });

            // The bar itself:
            column.Children.Add(new BoxView
            {
                Color = Color.FromArgb("#4CAF50"),
                WidthRequest = 30,
                HeightRequest = Math.Max(barHeight, 4), // Minimum 4px so that 0 values are visible
                HorizontalOptions = LayoutOptions.Center,
                CornerRadius = 3
            });

            // Date label below bar:
            column.Children.Add(new Label
            {
                Text = label,
                FontSize = 9,
                TextColor = Color.FromArgb("#AAAAAA"),
                HorizontalTextAlignment = TextAlignment.Center
            });

            ChartBarsLayout.Children.Add(column);
        }
    }

    // Data Transfer Objects (DTOs) for API responses:
    private class ExerciseLogDto
    {
        [JsonPropertyName("exerciseLogID")]
        public int ExerciseLogId { get; set; }
        [JsonPropertyName("workout")]
        public WorkoutDto? Workout { get; set; }
    }

    private class WorkoutDto
    {
        [JsonPropertyName("workoutTime")]
        public DateTime WorkoutTime { get; set; }
    }

    private class SetDto
    {
        [JsonPropertyName("setType")]
        public string SetType { get; set; } = string.Empty;
        [JsonPropertyName("reps")]
        public int Reps { get; set; }
        [JsonPropertyName("setWeightKG")]
        public decimal SetWeightKG { get; set; }
        [JsonPropertyName("distanceM")]
        public int DistanceM { get; set; }
        [JsonPropertyName("timeSeconds")]
        public int TimeSeconds { get; set; }
    }
}