namespace CaptainsLog.BoatProfilePages;

public partial class ProfileViewPage : ContentPage
{

    private readonly ViewModels.ProfilePageModel _profileViewModel;
    public ProfileViewPage()
    {
        InitializeComponent();
        BindingContext = _profileViewModel = new ViewModels.ProfilePageModel(new DatabaseClasses.ProfileJSONTools());
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _profileViewModel.LoadProfileItemsAsync();
        await _profileViewModel.CalulcateServiceHoursRemaining();
    }
}
