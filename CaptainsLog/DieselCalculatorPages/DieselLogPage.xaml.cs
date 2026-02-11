using CaptainsLog.DatabaseClasses.Services;
using CaptainsLog.ViewModels;
using System.Threading.Tasks;

namespace CaptainsLog;

public partial class DieselLogPage : ContentPage
{
    private readonly LogViewModel _logViewModel;

    public DieselLogPage()
	{
        InitializeComponent();
        BindingContext = _logViewModel = new LogViewModel(new DieselDatabaseMethods());

    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _logViewModel.LoadDatabaseItemsAsync();   
        await _logViewModel.LoadDateDropDown();
    }


}