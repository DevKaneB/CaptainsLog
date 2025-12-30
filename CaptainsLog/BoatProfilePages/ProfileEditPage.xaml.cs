using CaptainsLog.ViewModels;

namespace CaptainsLog.BoatProfilePages;

public partial class ProfileEditPage : ContentPage
{
    private readonly ViewModels.ProfilePageModel _profileEditViewModel;
    public ProfileEditPage()
	{
		InitializeComponent();
        BindingContext = _profileEditViewModel = new ViewModels.ProfilePageModel(new DatabaseClasses.Services.ProfileJSONTools());
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _profileEditViewModel.LoadProfileItemsAsync();
    }


}
