using NEAFitnessApp.Helpers;
using NEAFitnessApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
        // Set default Quantity to 1:
        QuantityEntry.Text = "1";

        // Add input validation for QuantityEntry:
        QuantityEntry.TextChanged += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
                return;

            // Only allow digits and one decimal point:
            string filtered = "";
            int decimalCount = 0;

            foreach (char c in e.NewTextValue)
            {
                if (c == '.')
                {
                    if (decimalCount == 0)
                    {
                        filtered += c;
                        decimalCount++;
                    }
                }
                else if (char.IsDigit(c))
                {
                    filtered += c;
                }
            }

            // Truncate to max 2 decimal places:
            if (filtered.Contains('.'))
            {
                string[] parts = filtered.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1 && parts[1].Length > 2)
                    filtered = parts[0] + "." + parts[1].Substring(0, 2);
            }

            // Update field with validated input for database storage:
            QuantityEntry.Text = filtered;
        };
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

                var sorted = SortingHelper.MergeSortChronological(logs);

                // Calculate totals using each log's own quantity
                foreach (var log in sorted)
                {
                    DailyLogs.Add(log);
                    cals += log.FoodItem.Calories * log.Quantity;
                    prot += log.FoodItem.Protein * log.Quantity;
                    fat += log.FoodItem.Fat * log.Quantity;
                    satFat += log.FoodItem.SaturatedFat * log.Quantity;
                    carb += log.FoodItem.Carbohydrates * log.Quantity;
                    sugar += log.FoodItem.Sugar * log.Quantity;
                    fibre += log.FoodItem.Fibre * log.Quantity;
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

        // Validate inputs before parsing
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(NewFoodNameEntry.Text))
            validationErrors.Add("Food name is required.");

        if (!int.TryParse(NewFoodCalsEntry.Text, out int calories) || calories < 0)
            validationErrors.Add("Please enter a valid calorie value.");

        if (!decimal.TryParse(NewFoodProteinEntry.Text, out decimal protein) || protein < 0)
            validationErrors.Add("Please enter a valid protein value.");

        if (!decimal.TryParse(NewFoodFatEntry.Text, out decimal fat) || fat < 0)
            validationErrors.Add("Please enter a valid fat value.");

        if (!decimal.TryParse(NewFoodSatFatEntry.Text, out decimal satFat) || satFat < 0)
            validationErrors.Add("Please enter a valid saturated fat value.");

        if (!decimal.TryParse(NewFoodCarbEntry.Text, out decimal carbs) || carbs < 0)
            validationErrors.Add("Please enter a valid carbohydrate value.");

        if (!decimal.TryParse(NewFoodSugarEntry.Text, out decimal sugar) || sugar < 0)
            validationErrors.Add("Please enter a valid sugar value.");

        if (!decimal.TryParse(NewFoodFibreEntry.Text, out decimal fibre) || fibre < 0)
            validationErrors.Add("Please enter a valid fibre value.");

        if (validationErrors.Any())
        {
            await DisplayAlert("Validation Error", string.Join("\n", validationErrors), "OK");
            return;
        }

        try
        {
            var newFood = new FoodItem
            {
                FoodName = NewFoodNameEntry.Text,
                Calories = calories,
                Protein = protein,
                Fat = fat,
                SaturatedFat = satFat,
                Carbohydrates = carbs,
                Sugar = sugar,
                Fibre = fibre,
                Quantity = decimal.Parse(QuantityEntry.Text),
                CreatedByUserID = userId,
            };

            var response = await _httpClient.PostAsJsonAsync("api/food/create", newFood);
            if (response.IsSuccessStatusCode)
            {
                await LoadAllFoods(); // Refresh and re-sort the table
                
                // Clear form fields
                NewFoodNameEntry.Text = "";
                NewFoodCalsEntry.Text = "";
                NewFoodProteinEntry.Text = "";
                NewFoodFatEntry.Text = "";
                NewFoodSatFatEntry.Text = "";
                NewFoodCarbEntry.Text = "";
                NewFoodSugarEntry.Text = "";
                NewFoodFibreEntry.Text = "";

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
                decimal totalCalories = response.Sum(log => log.FoodItem.Calories * log.Quantity);
                decimal average = totalCalories / 7;
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

        var logDate = LogDatePicker.Date; // Allow user to log to previous days.
        var logTimeOfDay = LogTimePicker.Time; // Allow user to pick custom time to log to.
        var logDateTime = logDate + logTimeOfDay; // Combine date and time for database storage.

        // Parse the quantity from QuantityEntry, default to 1 if empty:
        string quantityText = string.IsNullOrWhiteSpace(QuantityEntry.Text) ? "1" : QuantityEntry.Text;
        if (!decimal.TryParse(quantityText, out decimal quantity))
        {
            await DisplayAlert("Error", "Please enter a valid quantity.", "OK");
            return;
        }

        // Create the log object to send to the backend:
        var newLog = new FoodLogEntry
        {
            UserID = userId,
            FoodItemID = selectedFood.FoodItemId,
            LogTime = logDateTime,
            Quantity = quantity
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/food/log", newLog);

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