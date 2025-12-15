using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaptainsLog.DatabaseClasses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

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

        [RelayCommand]
        public async Task LoadExpensesItems()
        {
            var items = await expensesSQLTools.GetItemsAsync();

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
    }
}
