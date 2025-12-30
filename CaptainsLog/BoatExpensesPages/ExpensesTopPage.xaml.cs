using CaptainsLog.BoatProfilePages;
using CaptainsLog.DatabaseClasses.Services;
using System.Diagnostics;

namespace CaptainsLog.BoatExpensesPages;

public partial class ExpensesTopPage : ContentPage
{
    ExpensesSQLTools SQLTools;

	public ExpensesTopPage()
	{
		InitializeComponent();
        SQLTools = new ExpensesSQLTools();
    }

    async void OnWeeklyExpensesClicked(object? sender, EventArgs e)
    {
        try
        {
            var AmountItem = await SQLTools.GetItemsViaQueryAsync("SELECT printf('%.2f',SUM(Amount) / 3) * 1 AS AMOUNT From ExpensesItem Where ExpenseDate > DATE('now','-21 days')");
            AvgExpensesLbl.Text = AmountItem[0].Amount.ToString("F2") + "p/w";
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
            AvgExpensesLbl.Text = AmountItem[0].Amount.ToString("F2") + "p/m";
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
            AvgExpensesLbl.Text = AmountItem[0].Amount.ToString("F2") + "p/y";
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
        var window = Application.Current?.Windows.FirstOrDefault();
        var navigation = window?.Page?.Navigation;
        if (navigation != null)
        {
            await navigation.PushAsync(new ViewExpensesPage());
        }
    }
}