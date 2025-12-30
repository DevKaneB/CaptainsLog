using CaptainsLog.DatabaseClasses.Services;
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
		if (_ExpensesViewModel.FilterApplied == false)
		{
			await _ExpensesViewModel.LoadExpensesItems();
		}
		await _ExpensesViewModel.LoadDateDropDown();
    }

    

}