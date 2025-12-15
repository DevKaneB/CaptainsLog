using System;
using System.Globalization;
using CaptainsLog.BoatProfilePages;
using CaptainsLog.DatabaseClasses;

namespace CaptainsLog.BoatExpensesPages;

public partial class AddExpenseItemPage : ContentPage
{
	private ExpensesSQLTools database;
	private List<ExpensesItem>? ExpenseItems;
    public AddExpenseItemPage()
	{
		InitializeComponent();
		database = new ExpensesSQLTools();
    }

    async void OnSaveExpenseClicked(object? sender, EventArgs e)
    {
        //Warning for null, the syncfusion control forces a value
        DateTime date = ExpenseDateEntry.SelectedDate.Value;
        var dateSelected = date.ToString("yyyy-MM-dd");

        //Check for a number, they are restricted what they input via the XAML
        if (ExpenseAmountEntry.Value < 0.01 )
        {
            await DisplayAlert("Alert", "Amount is required.", "OK");
            return;
        }

        //Make sure there is a reason for the expense
        if (string.IsNullOrWhiteSpace(ExpenseReasonEntry.Text))
        {
            await DisplayAlert("Alert", "Please enter a reason for the expense.", "OK");
            return;
        }

        // Make sure a type is selected
        if (TypePicker == null || TypePicker.SelectedIndex == -1)
        {
            await DisplayAlert("Alert", "Please select a type for the expense.", "OK");
            return;
        }

        // Ensure ExpenseItems is initialized
        if (ExpenseItems == null)
        {
            ExpenseItems = new List<ExpensesItem>();
        }

        // Confirm user wants to add
        bool answer = await DisplayAlert("Confirm Save", "Are you sure you want to add this expense?", "Yes", "No");
        if (!answer)
            return;

        // Add the new expense item
        ExpenseItems.Add(new ExpensesItem
        {
            ExpenseType = TypePicker.Items[TypePicker.SelectedIndex],
            ExpenseDesc = ExpenseReasonEntry.Text,
            ExpenseDate = dateSelected,
            ID = 0,
            Amount = (decimal)(ExpenseAmountEntry.Value ?? 0.0)
        });

        //Save to database
        await database.SaveItemAsync(ExpenseItems[0]);
        ExpenseItems.Clear();
    }
}