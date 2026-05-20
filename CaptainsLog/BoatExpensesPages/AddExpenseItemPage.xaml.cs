using System;
using System.Diagnostics;
using System.Globalization;
using CaptainsLog.BoatProfilePages;
using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;

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
        await DisplayAlert("Help", "Please select the date you wish to add expenses to, choose the expense type and enter the amount. Then save your expense.", "OK");
        return;
    }

    async void OnSaveExpenseClicked(object? sender, EventArgs e)
    {
        // Validate date selection (SelectedDate is nullable)
        var selectedDate = ExpenseDateEntry?.SelectedDate;
        if (!selectedDate.HasValue)
        {
            await DisplayAlert("Alert", "Please select a date for the expense.", "OK");
            return;
        }
        DateTime date = selectedDate.Value;
        var dateSelected = date.ToString("yyyy-MM-dd");

        // Get amount safely (Value may be nullable)
        double amountDouble = ExpenseAmountEntry?.Value ?? 0.0;
        if (amountDouble < 0.01 )
        {
            await DisplayAlert("Alert", "Amount is required.", "OK");
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

        //Check Expense Item doesnt match existing entry
        ExpenseItems = await database.GetItemsViaQueryAsync($"SELECT * " +
                                                            $"FROM ExpensesItem " +
                                                            $"WHERE ExpenseType = '{TypePicker.Items[TypePicker.SelectedIndex]}' " +
                                                            $"AND (ExpenseDesc = '{ExpenseReasonEntry.Text}' OR ExpenseDesc IS NULL) " +
                                                            $"AND ExpenseDate = '{dateSelected}' " +
                                                            $"AND Amount = {(decimal)amountDouble}");

        if (ExpenseItems.Count > 0)
        {
            bool answer = await DisplayAlert("Confirm Save", "An identical expense item already exists. Do you wish to save anyway", "Yes", "No");
            if (!answer)
            {
                ExpenseItems.Clear();
                return;
            }
        } else
        {
            // Confirm user wants to add
            bool answer = await DisplayAlert("Confirm Save", "Are you sure you want to add this expense?", "Yes", "No");
            if (!answer)
            {
                ExpenseItems.Clear();
                return;
            }
        }

            // Add the new expense item
            ExpenseItems.Add(new ExpensesItem
            {
                ExpenseType = TypePicker.Items[TypePicker.SelectedIndex],
                ExpenseDesc = ExpenseReasonEntry.Text,
                ExpenseDate = dateSelected,
                ID = 0,
                Amount = (decimal)amountDouble
            });

        //Save to database
        await database.SaveItemAsync(ExpenseItems[0]);
        ExpenseItems.Clear();

        //Show confirmation and return to previous page
        await DisplayAlert("Success", "Expense item added successfully.", "OK");
    }
}