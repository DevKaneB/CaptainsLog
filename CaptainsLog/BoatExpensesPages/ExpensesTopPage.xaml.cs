using CaptainsLog.BoatProfilePages;

namespace CaptainsLog.BoatExpensesPages;

public partial class ExpensesTopPage : ContentPage
{
	public ExpensesTopPage()
	{
		InitializeComponent();
	}

    async void OnWeeklyExpensesClicked(object? sender, EventArgs e)
    {
        
    }

    async void OnMonthlyExpensesClicked(object? sender, EventArgs e)
    {

    }

    async void OnYearlyExpensesClicked(object? sender, EventArgs e)
    {

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
    }
}