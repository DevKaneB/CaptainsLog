namespace CaptainsLog
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        async void OnCounterClicked(object? sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PushAsync(
                new DieselCalcPage());
        }

    }
}
