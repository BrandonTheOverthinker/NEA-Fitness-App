using System.Net.Http.Json;
using System.Text.Json;

namespace NEAFitnessApp;

public partial class CreateWeightGoalPage : ContentPage
{
    private readonly int userId;
    private readonly string goalType;
    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CreateWeightGoalPage(int userId, string goalType)
    {
        InitializeComponent();
        this.userId = userId;
        this.goalType = goalType;

        PageTitle.Text = $"Create {goalType} Goal";
        DeadlinePicker.MinimumDate = DateTime.Today.AddDays(1);
        DeadlinePicker.MaximumDate = DateTime.Today.AddMonths(12);
        DeadlinePicker.Date = DateTime.Today.AddMonths(3);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserCurrentWeight();
    }

    private async Task LoadUserCurrentWeight()
    {
        try
        {
            var user = await _httpClient.GetFromJsonAsync<JsonElement>($"api/user/{userId}", JsonOpts);
            
            if (user.ValueKind != JsonValueKind.Null && user.TryGetProperty("bodyWeight", out var weightProperty))
            {
                if (decimal.TryParse(weightProperty.ToString(), out decimal currentWeight))
                {
                    StartWeightEntry.Text = currentWeight.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user weight: {ex.Message}");
        }
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            errors.Add("Description is required.");

        if (!decimal.TryParse(TargetWeightEntry.Text, out decimal targetWeight) || targetWeight <= 0)
            errors.Add("Target weight must be a valid positive number.");

        if (!decimal.TryParse(StartWeightEntry.Text, out decimal startWeight) || startWeight <= 0)
            errors.Add("Start weight must be a valid positive number.");

        if (goalType == "Weight Loss" && targetWeight >= startWeight)
            errors.Add("Target weight must be less than start weight for weight loss.");

        if (goalType == "Weight Gain" && targetWeight <= startWeight)
            errors.Add("Target weight must be greater than start weight for weight gain.");

        if (errors.Any())
        {
            await DisplayAlert("Validation Error", string.Join("\n", errors), "OK");
            return;
        }

        try
        {
            var request = new
            {
                UserId = userId,
                Description = DescriptionEntry.Text.Trim(),
                Deadline = DeadlinePicker.Date,
                TargetWeight = targetWeight,
                StartWeight = startWeight
            };

            var response = await _httpClient.PostAsJsonAsync("api/goal/weight", request);

            var responseBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Create weight goal -> {(int)response.StatusCode} {response.ReasonPhrase}; Body: '{responseBody}'");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", $"{goalType} goal created!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                string message = $"Failed to create goal: {(int)response.StatusCode} {response.ReasonPhrase}";
                if (!string.IsNullOrWhiteSpace(responseBody))
                    message += $"\n\nDetails: {responseBody}";

                await DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception posting goal: {ex.Message}");
            await DisplayAlert("Connection Error", ex.Message, "OK");
        }
    }
}