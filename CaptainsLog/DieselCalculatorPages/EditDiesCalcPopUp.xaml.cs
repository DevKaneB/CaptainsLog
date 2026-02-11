using CommunityToolkit.Maui.Views;

namespace CaptainsLog.DieselCalculatorPages
{
    public partial class EditDiesCalcPopUp : Popup<DieselHoursResult>
    {
        // Default popup size (adjust as needed)
        private const double DefaultPopupWidth = 280;
        private const double DefaultPopupHeight = 360;

        public EditDiesCalcPopUp()
        {
            InitializeComponent();
            WidthRequest = DefaultPopupWidth;
            HeightRequest = DefaultPopupHeight;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var result = new DieselHoursResult
            {
                PropHours = int.TryParse(PropHoursEntry.Text, out int propHours) ? propHours : 0,
                LeisHours = int.TryParse(LeisHoursEntry.Text, out int leisHours) ? leisHours : 0,
                DieselLitres = int.TryParse(LitresEntry.Text, out int dieselLitres) ? dieselLitres : 0
            };
            await CloseAsync(result);
        }
    }

    
}