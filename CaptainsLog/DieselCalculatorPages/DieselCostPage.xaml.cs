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
            InputLbl.Text = "Input the Cash you wish to spend";

            CashOrLitres = "L";
        } else
        {
            FlipCashBtn.Text = "Cost";
            sfCashLitreEntry.Placeholder = "ENTER LITRES";
            CalculateBtn.Text = "Calculate Cost";
            sfOutputEntry.Placeholder = "Cost Output";

            CashOrLitres = "C";
            InputLbl.Text = "Input the Litres you wish to buy";
        }
        
    }

    async void OnCalcClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sfDomesticCostEntry.Value is null || sfPropulsionCostEntry.Value is null ||
                sfDomesticDeclareEntry.Value is null || sfPropulsionDeclareEntry.Value is null ||
                sfCashLitreEntry.Value is null)
            {
                await DisplayAlert("Alert", "Please complete all fields before calculating.", "OK");
                return;
            }

            var domesticCost = Convert.ToDouble(sfDomesticCostEntry.Value);
            var propulsionCost = Convert.ToDouble(sfPropulsionCostEntry.Value);
            var domesticDeclare = Convert.ToDouble(sfDomesticDeclareEntry.Value);
            var propulsionDeclare = Convert.ToDouble(sfPropulsionDeclareEntry.Value);
            var cashOrLitresValue = Convert.ToDouble(sfCashLitreEntry.Value);

            if (domesticCost <= 0 || propulsionCost <= 0 || cashOrLitresValue <= 0)
            {
                await DisplayAlert("Alert", "Costs and input value must be greater than zero.", "OK");
                return;
            }

            if (domesticDeclare < 0 || propulsionDeclare < 0 ||
                Math.Abs((domesticDeclare + propulsionDeclare) - 100) > 0.0001)
            {
                await DisplayAlert("Alert", "Domestic and propulsion percentages must add up to 100%.", "OK");
                return;
            }

            var weightedPricePerLitre =
                (domesticCost * (domesticDeclare / 100)) +
                (propulsionCost * (propulsionDeclare / 100));

            if (CashOrLitres == "C")
            {
                var totalCost = cashOrLitresValue * weightedPricePerLitre;
                sfOutputEntry.Value = Math.Round(totalCost, 2);
            }
            else
            {
                var affordableLitres = cashOrLitresValue / weightedPricePerLitre;
                sfOutputEntry.Value = Math.Round(affordableLitres, 2);
            }
        }
        catch
        {
            await DisplayAlert("Alert", "Unable to calculate with the values provided.", "OK");
        }
    }
}