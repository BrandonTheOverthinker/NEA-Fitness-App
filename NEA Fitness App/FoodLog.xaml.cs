using System.Collections.ObjectModel;
using System.Net.Http.Json;
using NEAFitnessApp.Models;
using NEAFitnessApp.Helpers;

namespace NEAFitnessApp;

public partial class FoodLog : ContentPage
{
    // The lists for the UI
    public ObservableCollection<FoodItem> AllFoodsDB { get; set; } = new();
    public ObservableCollection<FoodLogEntry> DailyLogs { get; set; } = new();

    // Summary totals
    public string TotalCalories { get; set; } = "0";
    public string TotalProtein { get; set; } = "0";

    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };

    public FoodLog()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await Task.WhenAll(
                LoadAllFoods(),
                LoadDailyLogs(DateTime.Today),
                CalculateWeeklyAverage()
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading page data: {ex.Message}");
        }
    }

    // Load foods sorted alphabetically:
    private async Task LoadAllFoods()
    {
        try
        {
            var foods = await _httpClient.GetFromJsonAsync<List<FoodItem>>("api/food/all");
            if (foods != null)
            {
                var sorted = SortingHelper.MergeSortAlphabetical(foods);
                AllFoodsDB.Clear();
                foreach (var f in sorted) AllFoodsDB.Add(f);
            }
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    // Load logs for current date with totals:
    private async Task LoadDailyLogs(DateTime date)
    {
        int userId = Preferences.Get("CurrentUserID", 0);
        try
        {
            var logs = await _httpClient.GetFromJsonAsync<List<FoodLogEntry>>($"api/food/logs/{userId}/{date:yyyy-MM-dd}");
            if (logs != null)
            {
                DailyLogs.Clear();
                decimal cals = 0;
                decimal prot = 0;
                var sorted = SortingHelper.MergeSortChronological(logs);

                foreach (var log in sorted)
                {
                    DailyLogs.Add(log);
                    cals += log.FoodItem.Calories;
                    prot += log.FoodItem.Protein;
                }

                TotalCalories = cals.ToString("F0");
                TotalProtein = prot.ToString("F1");

                OnPropertyChanged(nameof(TotalCalories));
                OnPropertyChanged(nameof(TotalProtein));
            }
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    // CREATE NEW FOOD
    private async void OnCreateFoodClicked(object sender, EventArgs e)
    {
        // ADD EXCEPTION HANDLING HERE
        var newFood = new FoodItem
        {
            FoodName = NewFoodNameEntry.Text,
            Calories = int.Parse(NewFoodCalsEntry.Text),
            Protein = decimal.Parse(NewFoodProteinEntry.Text)
        };

        var response = await _httpClient.PostAsJsonAsync("api/food/create", newFood);
        if (response.IsSuccessStatusCode)
        {
            await LoadAllFoods(); // Refresh and re-sort the table
            await DisplayAlert("Success", "Food added to database", "OK");
        }
    }

    private async void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        await LoadDailyLogs(e.NewDate);
    }
    private string _weeklyAverageDisplay = "0";
    public string WeeklyAverageDisplay
    {
        get => _weeklyAverageDisplay;
        set
        {
            _weeklyAverageDisplay = value;
            OnPropertyChanged(); // Refresh XAML label
        }
    }
    private async Task CalculateWeeklyAverage()
    {
        int userId = Preferences.Get("CurrentUserID", 0);
        if (userId <= 0) return;

        // Get average for the last 7 days ending today:
        DateTime startDate = DateTime.Today.AddDays(-7);

        try
        {
            
            var response = await _httpClient.GetFromJsonAsync<List<FoodLogEntry>>(
                $"api/food/weekly/{userId}/{startDate:yyyy-MM-dd}"); // Fetch data from controller endpoint

            if (response != null && response.Count > 0)
            {
                decimal totalCalories = response.Sum(log => log.FoodItem.Calories);
                decimal average = totalCalories / 7;
                WeeklyAverageDisplay = average.ToString("F0"); // Update the UI property to 0 dp
            }
            else
            {
                WeeklyAverageDisplay = "0";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Average Calculation Error: {ex.Message}");
            WeeklyAverageDisplay = "Error";
        }
    }
    private async void OnLogFoodClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var selectedFood = (FoodItem)button.CommandParameter;

        if (selectedFood == null) return;

        // Get the current user ID (saved during login/registration)
        int userId = Preferences.Get("CurrentUserID", 0);
        if (userId == 0)
        {
            await DisplayAlert("Error", "User not found. Please log in again.", "OK");
            return;
        }

        // Create the log object to send to the backend
        var newLog = new FoodLogEntry
        {
            UserID = userId,
            FoodItemID = selectedFood.FoodItemId,
            LogTime = DateTime.Now,
            Quantity = 1.0m // Defaulting to 1 serving
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/log", newLog);

            if (response.IsSuccessStatusCode)
            {
                await LoadDailyLogs(DateTime.Today);
                await CalculateWeeklyAverage();

                await DisplayAlert("Success", $"{selectedFood.FoodName} logged!", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", "Failed to log food. " + error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", ex.Message, "OK");
        }
    }
}