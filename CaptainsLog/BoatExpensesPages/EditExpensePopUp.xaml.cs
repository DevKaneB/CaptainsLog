using CommunityToolkit.Maui.Views;

namespace CaptainsLog.BoatExpensesPages;

public partial class EditExpensePopup : Popup<ExpenseResult>
{
    // Default popup size (adjust as needed)
    private const double DefaultPopupWidth = 280;
    private const double DefaultPopupHeight = 360;

    public EditExpensePopup()
    {
        InitializeComponent();
        WidthRequest = DefaultPopupWidth;
        HeightRequest = DefaultPopupHeight;

    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var result = new ExpenseResult
        {
            ExpenseType = TypePicker.SelectedItem?.ToString() ?? string.Empty,
            ExpenseDesc = ExpenseDescEntry.Text,
            Amount = AmountEntry.Text
        };

        await CloseAsync(result);
    }
}

