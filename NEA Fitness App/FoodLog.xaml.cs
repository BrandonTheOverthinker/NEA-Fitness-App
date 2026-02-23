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
            // This runs all three methods simultaneously
            await Task.WhenAll(
                LoadAllFoods(),
                LoadDailyLogs(DateTime.Today),
                CalculateWeeklyAverage() // This removes the "low opacity" status!
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading page data: {ex.Message}");
        }
    }

    // LOAD ALL FOODS + SORT A-Z
    private async Task LoadAllFoods()
    {
        try
        {
            var foods = await _httpClient.GetFromJsonAsync<List<FoodItem>>("api/food/all");
            if (foods != null)
            {
                var sorted = SortingHelper.MergeSort(foods); // Your Merge Sort!
                AllFoodsDB.Clear();
                foreach (var f in sorted) AllFoodsDB.Add(f);
            }
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    // LOAD LOGS FOR SPECIFIC DATE + TOTALS
    private async Task LoadDailyLogs(DateTime date)
    {
        int userId = Preferences.Get("CurrentUserID", 0);
        try
        {
            var logs = await _httpClient.GetFromJsonAsync<List<FoodLogEntry>>($"api/food/logs/{userId}/{date:yyyy-MM-dd}");
            if (logs != null)
            {
                // Note: You can add a MergeSort variant for LogTime if you want logs chronological!
                DailyLogs.Clear();
                decimal cals = 0;
                decimal prot = 0;

                foreach (var log in logs)
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
        catch { /* Handle errors */ }
    }

    // CREATE NEW FOOD
    private async void OnCreateFoodClicked(object sender, EventArgs e)
    {
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
            OnPropertyChanged(); // This tells the XAML to refresh the label
        }
    }
    private async Task CalculateWeeklyAverage()
    {
        int userId = Preferences.Get("CurrentUserID", 0);
        if (userId <= 0) return;

        // We want the average for the LAST 7 days ending today
        DateTime startDate = DateTime.Today.AddDays(-7);

        try
        {
            // 1. Fetch the data from your new controller endpoint
            var response = await _httpClient.GetFromJsonAsync<List<FoodLogEntry>>(
                $"api/food/weekly/{userId}/{startDate:yyyy-MM-dd}");

            if (response != null && response.Count > 0)
            {
                // 2. Sum up all calories from all logs in that period
                decimal totalCalories = response.Sum(log => log.FoodItem.Calories);

                // 3. Divide by 7 to get the daily average
                decimal average = totalCalories / 7;

                // 4. Update the UI property (F0 removes decimals for a cleaner look)
                WeeklyAverageDisplay = average.ToString("F0");
            }
            else
            {
                WeeklyAverageDisplay = "0";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Average Calculation Error: {ex.Message}");
            WeeklyAverageDisplay = "ERR";
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
        // Note: Use the property names that match your Backend FoodLog model (UserID, FoodItemID, etc.)
        var newLog = new
        {
            UserID = userId,
            FoodItemID = selectedFood.FoodItemId,
            LogTime = DateTime.Now,
            Quantity = 1.0m // Defaulting to 1 serving
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/food/log", newLog);

            if (response.IsSuccessStatusCode)
            {
                // Refresh the daily logs and the weekly average to show the new data
                await LoadDailyLogs(DateTime.Today);
                await CalculateWeeklyAverage();

                await DisplayAlert("Success", $"{selectedFood.FoodName} logged!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to log food.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Connection error: " + ex.Message, "OK");
        }
    }
}