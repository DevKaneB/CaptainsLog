using CommunityToolkit.Maui.Views;

namespace CaptainsLog.BoatExpensesPages;

public partial class EditExpensePopup : Popup<ExpenseResult>
{
    public EditExpensePopup()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var result = new ExpenseResult
        {
            ExpenseDesc = ExpenseDescEntry.Text,
            Amount = AmountEntry.Text
        };

        await CloseAsync(result);
    }
}

