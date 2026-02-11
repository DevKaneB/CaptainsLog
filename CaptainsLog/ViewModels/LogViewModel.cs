using CaptainsLog.BoatExpensesPages;
using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using CaptainsLog.DieselCalculatorPages;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CaptainsLog.ViewModels
{
    public partial class LogViewModel : BaseViewModel
    {
        private readonly DieselDatabaseMethods _databaseClient;
        private bool FilterApplied = false;


        //Variable Declarations
        [ObservableProperty]
        public ObservableCollection<string>? dateDropDownData = new();

        [ObservableProperty]
        public ObservableCollection<DieselDatabase>? databaseItems = new();

        [ObservableProperty]
        public string selectedMonth = string.Empty;

        [ObservableProperty]
        public string selectedSort = string.Empty;

        //Cache Items
        private List<DieselDatabase>? cachedDatabaseItems;
        private DieselHoursResult? dieselHoursResult;

        //Load the date dropdown with months from the earliest expense date to now
        public async Task LoadDateDropDown()
        {
            cachedDatabaseItems = await _databaseClient.GetItemsViaQueryAsync("Select EntryDate FROM DieselDatabase where LeisureHours > 0 or PropHours > 0 or DieselRefill > 0 ORDER BY EntryDate ASC LIMIT 1");

            int monthDifference = GetMonthDifference(cachedDatabaseItems[0].EntryDate);
            if (monthDifference > 0)
            {

                var items = GetLastMonths(monthDifference);
                DateDropDownData = new ObservableCollection<string>(items);

            }

            cachedDatabaseItems = null;

        }

        //Calculate the number of months between now and the given date string
        private int GetMonthDifference(string dateString)
        {
            // Parse the input string into a DateTime
            DateTime inputDate = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            DateTime now = DateTime.Now;

            // Calculate difference in months
            int yearDiff = now.Year - inputDate.Year;
            int monthDiff = now.Month - inputDate.Month;

            int totalMonths = yearDiff * 12 + monthDiff;

            return totalMonths + 1;
        }

        public LogViewModel(DieselDatabaseMethods DatabaseClient)
        {
            _databaseClient = DatabaseClient;

            // initialize the explicit command so XAML compile-time/type-checking sees it
            DeleteEntryAsyncCommand = new AsyncRelayCommand<int>(async id =>
            {
                // Get the current page safely
                var page = Microsoft.Maui.Controls.Application.Current?.MainPage;

                // If no page is available, cancel the delete to avoid throwing
                if (page == null)
                    return;

                // Ask the user to confirm deletion
                var confirmed = await page.DisplayAlert(
                    "Confirm delete",
                    "Are you sure you want to delete this entry?",
                    "Yes",
                    "No");

                // If the user cancels, do nothing
                if (!confirmed)
                    return;

                // Proceed with deletion
                var itemToDelete = await _databaseClient.GetItemAsync(id);
                if (itemToDelete == null)
                    return;

                await _databaseClient.DeleteItemAsync(itemToDelete);
                if (FilterApplied == true)
                {
                    await ApplyFilters();
                }
                else
                {
                    await LoadDatabaseItemsAsync();
                }
            });

        }

        [RelayCommand]
        public async Task EditLog(int ID)
        {
            var popup = new EditDiesCalcPopUp();

            var page = Application.Current?.MainPage as Page;
            if (page == null)
                return;

            var result = await PopupExtensions.ShowPopupAsync<DieselHoursResult>(page, popup);

            if (result is not null)
            {
                // Store the popup's result in the view model field for later use
                if (result is IPopupResult<DieselHoursResult> popupResult && popupResult.Result is not null)
                {

                    dieselHoursResult = popupResult.Result;
                    dieselHoursResult.DieselHoursID = ID;

                    // Perform the 24-hour check before writing to the database
                    await TwentyFourHourCheck(dieselHoursResult);                    

                    dieselHoursResult = null;
                }
            }
        }

        private async Task TwentyFourHourCheck(DieselHoursResult dieselHoursResult)
        {
            if (dieselHoursResult.DieselHoursID != 0)
            {
                var item = await _databaseClient.GetItemAsync(dieselHoursResult.DieselHoursID);
                if (item != null)
                {
                    int totalHours = item.LeisureHours + item.PropHours;

                    // Remove unnecessary assignments to 'DieselHours' and 'PropHours'
                    int DieselHours = dieselHoursResult.LeisHours > 0 ? dieselHoursResult.LeisHours : item.LeisureHours;
                    int PropHours = dieselHoursResult.PropHours > 0 ? dieselHoursResult.PropHours : item.PropHours;

                    totalHours = DieselHours + PropHours;

                    if (totalHours > 24)
                    {
                        // Get the current page safely
                        var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
                        // If no page is available, cancel the delete to avoid throwing
                        if (page == null)
                            return;
                        // Alert the user that the total hours exceed 24
                        await page.DisplayAlert(
                            "Alert",
                            "The total hours for this entry exceed 24. Nothing has been updated.",
                            "Ok");
                    }
                    else
                    {
                        // If the total hours are within the limit, proceed to write to the database
                        await WriteEditToDatabase(dieselHoursResult);
                    }
                }
            }
        }
        private async Task WriteEditToDatabase(DieselHoursResult dhResult)
        {
            if (dhResult != null)
            {
                var itemToUpdate = await _databaseClient.GetItemAsync(dhResult.DieselHoursID);
                if (itemToUpdate != null)
                {

                    // Set all the values that were changed in the popup
                    // Only update if the value is not null or empty
                    if (dhResult.PropHours != 0)
                    {
                        itemToUpdate.PropHours = dhResult.PropHours;
                    }

                    if (dhResult.LeisHours != 0)
                    {
                        itemToUpdate.LeisureHours = dhResult.LeisHours;
                    }

                    if (dhResult.DieselLitres != 0)
                    {
                        itemToUpdate.DieselRefill = dhResult.DieselLitres;
                    }

                    //Update database
                    await _databaseClient.SaveItemAsync(itemToUpdate);


                    if (FilterApplied)
                    {
                        //Re-apply filters to show updated data
                        await ApplyFilters();
                    }
                    else
                    {
                        //Refresh to show everything
                        await LoadDatabaseItemsAsync();
                    }
                }
            }
        }

        [RelayCommand]
        public async Task OnBackButtonClicked()
        {
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var navigation = mainWindow?.Page?.Navigation;
            if (navigation != null)
            {
                await navigation.PopAsync();
            }
        }

        [RelayCommand]
        public async Task OnHelpButtonClicked()
        {
            // Get the current page safely
            var page = Microsoft.Maui.Controls.Application.Current?.MainPage;

            // If no page is available, cancel the delete to avoid throwing
            if (page == null)
                return;

            // Ask the user to confirm deletion
            await page.DisplayAlert(
                "Help",
                "View, sort and filter by month. Swipe left to delete or edit any values.",
                "Ok");  
            return;
        }

        [RelayCommand]
        public async Task ApplyFilters()
        {
            if(string.IsNullOrEmpty(SelectedMonth))
            {
                SelectedMonth = "All";
            }
            if(string.IsNullOrEmpty(SelectedSort))
            {
                SelectedSort = "Descending";
            }

            var SortOrder = SelectedSort == "Descending" ? "DESC" : "ASC";

            if (cachedDatabaseItems == null)
            {
                cachedDatabaseItems = await _databaseClient.GetItemsViaQueryAsync($"Select * from DieselDatabase where LeisureHours > 0 or PropHours > 0 or DieselRefill > 0 Order By EntryDate {SortOrder}");
            }

            var filteredList = cachedDatabaseItems.AsEnumerable();
            if (SelectedMonth != "All")
            {
                DateTime selectedDateTime = DateTime.ParseExact(SelectedMonth, "MMMM yyyy", CultureInfo.InvariantCulture);
                filteredList = filteredList.Where(item =>
                {
                    if (string.IsNullOrEmpty(item.EntryDate))
                        return false;
                    DateTime itemDate = DateTime.ParseExact(item.EntryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return itemDate.Year == selectedDateTime.Year && itemDate.Month == selectedDateTime.Month;
                });
            }

            DatabaseItems = new ObservableCollection<DieselDatabase>(filteredList);

            FilterApplied = true;

            cachedDatabaseItems = null;
        }


        [RelayCommand]
        public async Task LoadDatabaseItemsAsync()
        {
            var items = await _databaseClient.GetItemsViaQueryAsync("Select * from DieselDatabase where LeisureHours > 0 or PropHours > 0 or DieselRefill > 0 Order By EntryDate DESC");

            if (items.Count == 0)
            {
                // Get the current page safely
                var page = Microsoft.Maui.Controls.Application.Current?.MainPage;

                // If no page is available, cancel the delete to avoid throwing
                if (page == null)
                    return;

                // Ask the user to confirm deletion
                await page.DisplayAlert(
                    "Alert",
                    "There are no entries to show!",
                    "Ok");

                
            }
            DatabaseItems = new ObservableCollection<DieselDatabase>(items);
        }

        //Generate a list of month names for the last X months
        public static List<string> GetLastMonths(int AmountOfMonths)
        {
            var months = new List<string>();
            DateTime current = DateTime.Now;

            months.Add("All");

            for (int i = 0; i < AmountOfMonths; i++)
            {
                months.Add(current.AddMonths(-i).ToString("MMMM yyyy"));
            }

            return months;
        }


        // Provide an explicit command property that XAML can see at compile-time.
        // We create and wire it in the constructor above.
        public IAsyncRelayCommand<int> DeleteEntryAsyncCommand { get; }
    }
}
