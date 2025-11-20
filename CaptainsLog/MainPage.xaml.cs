using CaptainsLog.APIKeys;

namespace CaptainsLog
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            var apiKey = APIKeys.APIKeys.syncfusionKey;

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(apiKey);


            InitializeComponent();
        }

        async void OnCounterClicked(object? sender, EventArgs e)
        {
            // Fixes CS0618 and CS8602 by using the current window's page and null-checking
            var window = Application.Current?.Windows.FirstOrDefault();
            var navigation = window?.Page?.Navigation;
            if (navigation != null)
            {
                await navigation.PushAsync(new DieselCalcPage());
            }
            // Optionally, handle the case where navigation is null (e.g., show an error or do nothing)
        }
    }
}
