using CaptainsLog.DatabaseClasses;
using CaptainsLog.ViewModels;
using CommunityToolkit.Maui.Extensions;


namespace CaptainsLog.BoatExpensesPages;

public partial class ViewExpensesPage : ContentPage
{
	private readonly ExpensesViewModel _ExpensesViewModel;

    public ViewExpensesPage()
	{
		InitializeComponent();
		BindingContext = _ExpensesViewModel = new ExpensesViewModel(new ExpensesSQLTools());
    }

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		await _ExpensesViewModel.LoadExpensesItems();
		await _ExpensesViewModel.LoadDateDropDown();
    }

    private async void OnEditExpenseClicked(object sender, EventArgs e)
    {
        var popup = new EditExpensePopup();

        // result is IPopupResult<ExpenseResult>
        var result = await this.ShowPopupAsync<ExpenseResult>(popup);

        if (result is not null && result.Result is not null)
        {
            //save values to the database
        }
    }

}