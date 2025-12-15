using CaptainsLog.DatabaseClasses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.ViewModels
{
    public partial class ExpensesViewModel : BaseViewModel
    {
        private readonly ExpensesSQLTools expensesSQLTools;

        public ExpensesViewModel(ExpensesSQLTools expensesSQLTools)
        {
            this.expensesSQLTools = expensesSQLTools;
        }

        [ObservableProperty]
        public ObservableCollection<ExpensesItem>? expensesItems = new();

        [ObservableProperty]
        public ObservableCollection<string>? dateDropDownData = new();

        //This is for caching purposes
        private List<ExpensesItem>? expenseSQLList;

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

        public async Task LoadDateDropDown()
        {
            expenseSQLList = await expensesSQLTools.GetItemsViaQueryAsync("Select ExpenseDate FROM ExpensesItem ORDER BY Expensedate ASC LIMIT 1");

            int monthDifference = GetMonthDifference(expenseSQLList[0].ExpenseDate);
            if (monthDifference > 0) { 
            
                var items = GetLastMonths(monthDifference);
                DateDropDownData = new ObservableCollection<string>(items);

            }
        }

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
