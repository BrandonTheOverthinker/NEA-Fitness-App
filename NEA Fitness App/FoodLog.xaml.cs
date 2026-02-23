using System.Collections.ObjectModel;
using System.Net.Http.Json;
using NEAFitnessApp.Models;
using NEAFitnessApp.Helpers;

namespace NEAFitnessApp;

public partial class FoodLog : ContentPage
{
    public ObservableCollection<FoodItem> FoodSearchResults { get; set; } = new ObservableCollection<FoodItem>();

    // Use the same HttpClient setup as Registration page:
    private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7041/") };
     
    public FoodLog()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public async Task SearchForFood(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput)) return;

        try
        {
            // json logic:
            var rawList = await _httpClient.GetFromJsonAsync<List<FoodItem>>($"api/food/search/0?query={userInput}");

            if (rawList != null)
            {
                // Merge sort the raw json data:
                var sortedList = SortingHelper.MergeSort(rawList);

                // UI Logic:
                FoodSearchResults.Clear();
                foreach (var item in sortedList)
                {
                    FoodSearchResults.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to fetch and sort data: " + ex.Message, "OK");
        }
    }
}