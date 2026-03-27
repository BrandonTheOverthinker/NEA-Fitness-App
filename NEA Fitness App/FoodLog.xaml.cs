using System.Collections.ObjectModel;
using System.Net.Http.Json;
using NEAFitnessApp.Models;
using NEAFitnessApp.Helpers;
using System.Text.Json;
using System.Text;

namespace NEAFitnessApp;

public partial class FoodLog : ContentPage
{
    // Lists for the UI:
    public ObservableCollection<FoodItem> AllFoodsDB { get; set; } = new();
    public ObservableCollection<FoodLogEntry> DailyLogs { get; set; } = new();

    public string TotalCalories { get; set; } = "0";
    public string TotalProtein { get; set; } = "0";
    public string TotalFat { get; set; } = "0";
    public string TotalSatFat { get; set; } = "0";
    public string TotalCarbs { get; set; } = "0";
    public string TotalSugar { get; set; } = "0";
    public string TotalFibre { get; set; } = "0";

    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7281/") };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FoodLog()
    {
        InitializeComponent();
        BindingContext = this;

        // Set Default Log Date and Time:
        LogTimePicker.Time = DateTime.Now.TimeOfDay;
        LogDatePicker.Date = DateTime.Now.Date;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        int userId = Preferences.Get("CurrentUserID", 0);
        if (userId == 0)
        {
            await DisplayAlert("Error", "User not found. Please log in again.", "OK");
            return;
        }

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
                decimal fat = 0;
                decimal satFat = 0;
                decimal carb = 0;
                decimal sugar = 0;
                decimal fibre = 0;
                decimal quantity = Convert.ToDecimal(QuantityEntry.Text);
                var sorted = SortingHelper.MergeSortChronological(logs);

                // Get totals and multiply them by quantity
                foreach (var log in sorted)
                {
                    DailyLogs.Add(log);
                    cals += log.FoodItem.Calories * quantity;
                    prot += log.FoodItem.Protein * quantity;
                    fat += log.FoodItem.Fat * quantity;
                    satFat += log.FoodItem.SaturatedFat * quantity;
                    carb += log.FoodItem.Carbohydrates * quantity;
                    sugar += log.FoodItem.Sugar * quantity;
                    fibre += log.FoodItem.Fibre * quantity;
                }

                TotalCalories = cals.ToString("F0");
                TotalProtein = prot.ToString("F1");
                TotalFat = fat.ToString("F1");
                TotalSatFat = satFat.ToString("F1");
                TotalCarbs = carb.ToString("F1");
                TotalSugar = sugar.ToString("F1");
                TotalFibre = fibre.ToString("F1");

                OnPropertyChanged(nameof(TotalCalories));
                OnPropertyChanged(nameof(TotalProtein));
                OnPropertyChanged(nameof(TotalFat));
                OnPropertyChanged(nameof(TotalSatFat));
                OnPropertyChanged(nameof(TotalCarbs));
                OnPropertyChanged(nameof(TotalSugar));
                OnPropertyChanged(nameof(TotalFibre));
            }
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    private async void OnCreateFoodClicked(object sender, EventArgs e)
    {
        
        int userId = Preferences.Get("CurrentUserID", 0);
        if (userId == 0)
        {
            await DisplayAlert("Error", "User not found. Please log in again.", "OK");
            return;
        }
        try
        {
            var newFood = new FoodItem // ADD EXCEPTION HANDLING HERE!!!!!!!!!
            {
                FoodName = NewFoodNameEntry.Text,
                Calories = int.Parse(NewFoodCalsEntry.Text),
                Protein = decimal.Parse(NewFoodProteinEntry.Text),
                Fat = decimal.Parse(NewFoodFatEntry.Text),
                SaturatedFat = decimal.Parse(NewFoodSatFatEntry.Text),
                Carbohydrates = decimal.Parse(NewFoodCarbEntry.Text),
                Sugar = decimal.Parse(NewFoodSugarEntry.Text),
                Fibre = decimal.Parse(NewFoodFibreEntry.Text),
                Quantity = decimal.Parse(QuantityEntry.Text),
                CreatedByUserID = userId,
            };

            var response = await _httpClient.PostAsJsonAsync("api/food/create", newFood);
            if (response.IsSuccessStatusCode)
            {
                await LoadAllFoods(); // Refresh and re-sort the table
                await DisplayAlert("Success", "Food added to database", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", "Failed to add food to database: " + error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", ex.Message, "OK");
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
    private async void OnLogFoodClicked(object sender, EventArgs e) // LOG NOT CURRENTLY WORKING, FIX BEFORE OR DURING TESTING!
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

        var logDate = LogDatePicker.Date; // Allow user to log to previous days.
        var logTimeOfDay = LogTimePicker.Time; // Allow user to pick custom time to log to.
        var logDateTime = logDate + logTimeOfDay; // Combine date and time for database storage.

        // Create the log object to send to the backend:
        var newLog = new FoodLogEntry
        {
            UserID = userId,
            FoodItemID = selectedFood.FoodItemId,
            LogTime = logDateTime,
            Quantity = 1.0m // Defaulting to 1 serving
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/food/log", newLog);
            var client = new HttpClient();
            var json = JsonSerializer.Serialize(newLog);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            if (response.IsSuccessStatusCode)
            {
                var FoodLog = JsonSerializer.Deserialize<FoodLogEntry>(json, JsonOpts);

                if (FoodLog != null)
                {
                    await Navigation.PushAsync(new FoodLog());
                }
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