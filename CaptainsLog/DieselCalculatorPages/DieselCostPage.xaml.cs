namespace CaptainsLog.DieselCalculatorPages;

public partial class DieselCostPage : ContentPage
{

    private string CashOrLitres;

	public DieselCostPage()
	{
		InitializeComponent();
        CashOrLitres = "C";
	}

    async void OnBackButtonClicked(object? sender, EventArgs e)
    {
        var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        var navigation = mainWindow?.Page?.Navigation;
        if (navigation != null)
        {
            await navigation.PopAsync();
        }
    }

    async void OnHelpButtonClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Help", "Input the money you have, your domestic/propulsion declarations and the cost of diesel to see how much you can afford.", "OK");
        return;
    }

    async void OnCostLitreClicked(object? sender, EventArgs e)
    {
        if (CashOrLitres == "C")
        {
            FlipCashBtn.Text = "Litres";
            sfCashLitreEntry.Placeholder = "INPUT CASH";
            CalculateBtn.Text = "Calculate Litres";
            sfOutputEntry.Placeholder = "Affordable Litres";

            CashOrLitres = "L";
        } else
        {
            FlipCashBtn.Text = "Cost";
            sfCashLitreEntry.Placeholder = "ENTER LITRES";
            CalculateBtn.Text = "Calculate Cost";
            sfOutputEntry.Placeholder = "Cost Output";

            CashOrLitres = "C";
        }
        
    }

    async void OnCalcClicked(object? sender, EventArgs e)
    {
        
    }
}