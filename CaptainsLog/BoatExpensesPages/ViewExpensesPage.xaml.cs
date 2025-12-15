using CaptainsLog.DatabaseClasses;
using CaptainsLog.ViewModels;


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

}