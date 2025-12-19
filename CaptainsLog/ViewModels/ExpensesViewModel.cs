using CaptainsLog.BoatExpensesPages;
using CaptainsLog.DatabaseClasses;
using CaptainsLog.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Core;


namespace CaptainsLog.ViewModels
{
    public partial class ExpensesViewModel : BaseViewModel
    {
        private readonly ExpensesSQLTools expensesSQLTools;
        private readonly PdfService _pdfService = new PdfService();
        public bool FilterApplied;    

        public IRelayCommand ApplyFiltersCommand => new RelayCommand(async () => await ApplyFilters());
        public IRelayCommand ExportAsPDFCommand => new RelayCommand(async () => await ExportAsPDF());
        public IRelayCommand EditExpenseCommand => new RelayCommand<int>(async (id) => await EditExpense(id));


        public ExpensesViewModel(ExpensesSQLTools expensesSQLTools)
        {
            this.expensesSQLTools = expensesSQLTools;
            expenseResult = new ExpenseResult();
            FilterApplied = false;
        }

        [ObservableProperty]
        public string selectedType;

        [ObservableProperty]
        public string selectedMonth;

        [ObservableProperty]
        public ObservableCollection<ExpensesItem>? expensesItems = new();

        [ObservableProperty]
        public ObservableCollection<string>? dateDropDownData = new();

        //This is for caching purposes
        private List<ExpensesItem>? expenseSQLList;

        public ExpenseResult? expenseResult;

        //Load all expense items from the database
        public async Task LoadExpensesItems()
        {
            var items = await expensesSQLTools.GetItemsViaQueryAsync("Select * FROM ExpensesItem ORDER BY Expensedate DESC");

            if (items.Count == 0)
            {
                // Get the current page safely
                var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
                // If no page is available, cancel the alert to avoid throwing
                if (page == null)
                    return;
                // Inform the user that there are no expense items
                await page.DisplayAlert(
                    "Alert",
                    "There are currently nothing added to show!",
                    "OK");
            }


            ExpensesItems = new ObservableCollection<ExpensesItem>(items);
        }

        //Load the date dropdown with months from the earliest expense date to now
        public async Task LoadDateDropDown()
        {
            expenseSQLList = await expensesSQLTools.GetItemsViaQueryAsync("Select ExpenseDate FROM ExpensesItem ORDER BY Expensedate ASC LIMIT 1");

            int monthDifference = GetMonthDifference(expenseSQLList[0].ExpenseDate);
            if (monthDifference > 0) {

                var items = GetLastMonths(monthDifference);
                DateDropDownData = new ObservableCollection<string>(items);

            }

            expenseSQLList = null;

        }

        //Open the edit expense popup and handle the result
        public async Task EditExpense(int ID)
        {
            var popup = new EditExpensePopup();

            // Safely get the current Page (ShowPopupAsync is an extension for Page)
            var page = Application.Current?.MainPage as Page;
            if (page == null)
                return;

            // Call the extension method on the Page explicitly via the static helper to avoid CS1929
            var result = await PopupExtensions.ShowPopupAsync<ExpenseResult>(page, popup);

            if (result is not null)
            {
                // Store the popup's result in the view model field for later use
                if (result is IPopupResult<ExpenseResult> popupResult && popupResult.Result is not null)
                {
                    
                    expenseResult = popupResult.Result;
                    expenseResult.ExpenseID = ID;

                    await WriteEditToDatabase();

                    expenseResult = null;
                }
            }
        }

        //Write the edited expense data back to the database
        public async Task WriteEditToDatabase()
        {
            if (expenseResult != null)
            {
                var itemToUpdate = await expensesSQLTools.GetItemAsync(expenseResult.ExpenseID);
                if (itemToUpdate != null)
                {

                    // Set all the values that were changed in the popup
                    // Only update if the value is not null or empty
                    if (!string.IsNullOrEmpty(expenseResult.ExpenseType))
                    {
                        itemToUpdate.ExpenseType = expenseResult.ExpenseType;
                    }

                    if (!string.IsNullOrEmpty(expenseResult.ExpenseDesc))
                    {
                        itemToUpdate.ExpenseDesc = expenseResult.ExpenseDesc;
                    }

                    if (!string.IsNullOrEmpty(expenseResult.Amount))
                    {
                        // Fix: Convert string to decimal before assignment
                        if (decimal.TryParse(expenseResult.Amount, out var amount))
                        {
                            itemToUpdate.Amount = amount;
                        }
                        else
                        {
                            // Handle invalid input (e.g., set to 0 or show an error)
                            itemToUpdate.Amount = 0;
                        }
                    }

                    //Update database
                    await expensesSQLTools.SaveItemAsync(itemToUpdate);


                    if (FilterApplied)
                    {
                        //Re-apply filters to show updated data
                        await ApplyFilters();
                    } else
                    {
                        //Refresh to show everything
                        await LoadExpensesItems();
                    }  
                }
            }
        }


        //Export the current expenses list as a PDF
        private async Task ExportAsPDF()
        {
            // Get the current page safely
            var page = Microsoft.Maui.Controls.Application.Current?.MainPage;

            // If no page is available, cancel the delete to avoid throwing
            if (page == null)
                return;

            // Ask the user to confirm deletion
            var confirmed = await page.DisplayAlert(
                "Confirm Export",
                "Do you confirm you wish to Export as PDF?",
                "Yes",
                "No");

            // If the user cancels, do nothing
            if (!confirmed)
                return;

            List<ExpensesItem> items = ExpensesItems.ToList();

            var pdfService = new PdfService();
            string filePath = await pdfService.ExportListToPdfAsync(items);

            // Share the PDF
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Exported Products",
                File = new ShareFile(filePath)
            });

        }

        //Apply filters to the expenses list show on screen
        private async Task ApplyFilters()
        {

            if (string.IsNullOrEmpty(SelectedMonth))
            {
                SelectedMonth = "All";
            }

            if (string.IsNullOrEmpty(SelectedType))
            {
                SelectedType = "All";
            }

            if (expenseSQLList == null)
            {
                expenseSQLList = await expensesSQLTools.GetItemsViaQueryAsync("Select * FROM ExpensesItem ORDER BY Expensedate DESC");
            }
            var filteredList = expenseSQLList.AsEnumerable();
            if (SelectedMonth != "All")
            {
                DateTime selectedDateTime = DateTime.ParseExact(SelectedMonth, "MMMM yyyy", CultureInfo.InvariantCulture);
                filteredList = filteredList.Where(item =>
                {
                    if (string.IsNullOrEmpty(item.ExpenseDate))
                        return false;
                    DateTime itemDate = DateTime.ParseExact(item.ExpenseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return itemDate.Year == selectedDateTime.Year && itemDate.Month == selectedDateTime.Month;
                });
            }
            if (SelectedType != "All")
            {
                filteredList = filteredList.Where(item => item.ExpenseType == selectedType);
            }
            ExpensesItems = new ObservableCollection<ExpensesItem>(filteredList);

            FilterApplied = true;

            expenseSQLList = null;
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



    }
}
