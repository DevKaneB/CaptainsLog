using CaptainsLog.ViewModels;

namespace CaptainsLog.SettingsPages;

public partial class Set_MainPage : ContentPage
{

	private readonly SettingsViewModel _SettingsViewModel;

	public Set_MainPage()
	{
		InitializeComponent();
		BindingContext = _SettingsViewModel = new SettingsViewModel();
	}
}