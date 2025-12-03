using CaptainsLog.ViewModels;

namespace CaptainsLog.BoatProfilePages;

public partial class ProfileEditPage : ContentPage
{
    private readonly ViewModels.ProfilePageModel _profileViewModel;
    public ProfileEditPage()
	{
		InitializeComponent();
        BindingContext = _profileViewModel = new ViewModels.ProfilePageModel(new DatabaseClasses.ProfileJSONTools());
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _profileViewModel.LoadProfileItemsAsync();
    }


}
