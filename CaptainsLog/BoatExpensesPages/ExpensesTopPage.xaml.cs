using CaptainsLog.BoatProfilePages;
using CaptainsLog.DatabaseClasses.Services;
using CaptainsLog.DatabaseClasses.Items;
using System.Diagnostics;

namespace CaptainsLog.BoatExpensesPages;

public partial class ExpensesTopPage : ContentPage
{
    ExpensesSQLTools SQLTools;
    List<ExpensesItem> ExpenseItems = new List<ExpensesItem>();

    public ExpensesTopPage()
	{
		InitializeComponent();
        SQLTools = new ExpensesSQLTools();
    }

    async void OnBackButtonClicked(object? sender, EventArgs e)
    {
        var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        var navigation = mainWindow?.Page?.Navigation;
        if (navigation != null)
        {
            await navigation.PopAsync();
        }
    }

    async void OnHelpButtonClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Help", "The Expenses page allows you to view your average spending, log your expenses and export your expenses as a statement", "OK");
        return;
    }

    async void OnWeeklyExpensesClicked(object? sender, EventArgs e)
    {
        try
        {
            var AmountItem = await SQLTools.GetItemsViaQueryAsync("SELECT printf('%.2f',SUM(Amount) / 3) * 1 AS AMOUNT From ExpensesItem Where ExpenseDate > DATE('now','-21 days')");
            AvgExpensesLbl.Text = "£" + AmountItem[0].Amount.ToString("F2") + "p/w";
            AmountItem.Clear();

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    async void OnMonthlyExpensesClicked(object? sender, EventArgs e)
    {
        try
        {
            var AmountItem = await SQLTools.GetItemsViaQueryAsync("SELECT printf('%.2f',SUM(Amount) / 3) * 1 AS AMOUNT From ExpensesItem Where ExpenseDate > DATE('now','-90 days')");
            AvgExpensesLbl.Text = "£" + AmountItem[0].Amount.ToString("F2") + "p/m";
            AmountItem.Clear();

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);

        }
    }

    async void OnYearlyExpensesClicked(object? sender, EventArgs e)
    {
        try
        {
            var AmountItem = await SQLTools.GetItemsViaQueryAsync("SELECT printf('%.2f',SUM(Amount) / 3) * 1 AS AMOUNT From ExpensesItem Where ExpenseDate > DATE('now','-365 days')");
            AvgExpensesLbl.Text = "£" + AmountItem[0].Amount.ToString("F2") + "p/y";
            AmountItem.Clear();

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);

        }
    }

    async void OnAddExpenseClicked(object? sender, EventArgs e)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        var navigation = window?.Page?.Navigation;
        if (navigation != null)
        {
            await navigation.PushAsync(new AddExpenseItemPage());
        }
    }

    async void OnViewExpensesClicked(object? sender, EventArgs e)
    {
        ExpenseItems = await SQLTools.GetItemsViaQueryAsync("Select * FROM ExpensesItem ORDER BY Expensedate DESC");

        if (ExpenseItems.Count == 0)
        {
            // Get the current page safely
            var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
            // If no page is available, cancel the alert to avoid throwing
            if (page == null)
                return;
            // Inform the user that there are no expense items
            await page.DisplayAlert(
                "Alert",
                "There is currently nothing added to show!",
                "OK");

            return;
        }

        var window = Application.Current?.Windows.FirstOrDefault();
        var navigation = window?.Page?.Navigation;
        if (navigation != null)
        {
            await navigation.PushAsync(new ViewExpensesPage());
        }
    }
}