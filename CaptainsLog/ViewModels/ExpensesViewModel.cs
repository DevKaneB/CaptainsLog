using CaptainsLog.DatabaseClasses;
using CaptainsLog.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.ViewModels
{
    public partial class ExpensesViewModel : BaseViewModel
    {
        private readonly ExpensesSQLTools expensesSQLTools;
        private readonly PdfService _pdfService = new PdfService();

        public IRelayCommand ApplyFiltersCommand => new RelayCommand(async () => await ApplyFilters());
        public IRelayCommand ExportAsPDFCommand => new RelayCommand(async () => await ExportAsPDF());

        public ExpensesViewModel(ExpensesSQLTools expensesSQLTools)
        {
            this.expensesSQLTools = expensesSQLTools;

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
            if (expenseSQLList == null)
            {
                expenseSQLList = await expensesSQLTools.GetItemsViaQueryAsync("Select * FROM ExpensesItem ORDER BY Expensedate DESC");
            }
            var filteredList = expenseSQLList.AsEnumerable();
            if (selectedMonth != "All")
            {
                DateTime selectedDateTime = DateTime.ParseExact(selectedMonth, "MMMM yyyy", CultureInfo.InvariantCulture);
                filteredList = filteredList.Where(item =>
                {
                    DateTime itemDate = DateTime.ParseExact(item.ExpenseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return itemDate.Year == selectedDateTime.Year && itemDate.Month == selectedDateTime.Month;
                });
            }
            if (selectedType != "All")
            {
                filteredList = filteredList.Where(item => item.ExpenseType == selectedType);
            }
            ExpensesItems = new ObservableCollection<ExpensesItem>(filteredList);

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
