using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using Syncfusion.Maui.Picker;
using System.Diagnostics;

namespace CaptainsLog;

public partial class AddHoursPage : ContentPage
{
    private DieselDatabaseMethods database;
    private List<DieselDatabase>? databaseItems;

    public AddHoursPage()
	{
		InitializeComponent();
        database = new DieselDatabaseMethods();
        LoadDateHours();
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
        await DisplayAlert("Help", "Select the date you wish to update, then input the total amount of hours for this day and update", "OK");
        return;
    }

    async void LoadDateHours()
    {
        try
        {
            DateTime date = HoursDatePicker.SelectedDate.Value;
            var dateSelected = date.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");
            switch (databaseItems.Count)
            {
                // If database has no records for that date, clear the entries
                case 0:
                    sfDiesEntry.Value = null;
                    sfPropEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesEntry.Value = databaseItems[0].LeisureHours;
                    sfPropEntry.Value = databaseItems[0].PropHours;
                    break;
                //Should never see this, it is an error
                default:
                    Debug.WriteLine("More than 1 item found for this date");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    async void OnDateChangedEvent(object sender, DatePickerSelectionChangedEventArgs e)
    {
        try
        {
            DateTime selectedDate = (DateTime)e.NewValue;
            var dateSelected = selectedDate.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");
            switch (databaseItems.Count)
            {
                // If database has no records for that date, clear the entries
                case 0:
                    sfDiesEntry.Value = null;
                    sfPropEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesEntry.Value = databaseItems[0].LeisureHours;
                    sfPropEntry.Value = databaseItems[0].PropHours;
                    break;
                //Should never see this, it is an error
                default:
                    Debug.WriteLine("More than 1 item found for this date");
                    break;
            }

        } catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    }

    async void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        try
        {
            DateTime date = HoursDatePicker.SelectedDate.Value;
            var dateSelected = date.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");
            switch (databaseItems.Count)
            {
                // If database has no records for that date, clear the entries
                case 0:
                    sfDiesEntry.Value = null;
                    sfPropEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesEntry.Value = databaseItems[0].LeisureHours;
                    sfPropEntry.Value = databaseItems[0].PropHours;
                    break;
                //Should never see this, it is an error
                default:
                    Debug.WriteLine("More than 1 item found for this date");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    async void OnAddHoursClicked(object? sender, EventArgs e)
    {
        try
        {
            //Check field inputs are different from what is already in the database for this date
            //Get users date from the form
            DateTime date = HoursDatePicker.SelectedDate.Value;
            var dateSelected = date.ToString("yyyy-MM-dd");
            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");
            switch (databaseItems.Count)
            {
                // If database has no records for that date, do nothing
                case 0:
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    if (sfDiesEntry.Value == databaseItems[0].LeisureHours &&
                        sfPropEntry.Value == databaseItems[0].PropHours)
                    {
                        await DisplayAlert("Alert", "No changes detected to the hours for this date", "OK");
                        return;
                    }
                    break;
                //Should never see this, it is an error
                default:
                    Debug.WriteLine($"More than 1 item found for this date: {dateSelected}");
                    break;
            }

            //check user has entered at least one value
            if (sfDiesEntry.Value < 1 && sfPropEntry.Value < 1)
            {
                await DisplayAlert("Alert", "Please enter hours for either Diesel or Leisure", "OK");
                return;
            }

            //Set any empty entries to 0
            if (sfDiesEntry.Value < 1)
                sfDiesEntry.Value = 0;
            //set any empty entries to 0
            if (sfPropEntry.Value < 1)
                sfPropEntry.Value = 0;

            //Confirm user wants to add
            bool answer = await DisplayAlert("Confirm Add", "Are you sure you want to add these hours?", "Yes", "No");
            if (!answer)
                return;

            //Get users date from the form
            var continueWrite = true;

            //Check and get any entries from the database for this date, there should always be 1. 
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                // If database has no records for that date, insert a new record
                case 0:
                    //Check hours for this date are not above 24 hours
                    if (sfDiesEntry.Value + sfPropEntry.Value > 24)
                    {
                        await DisplayAlert("Alert", "Total amount of hours is above 24 for this date", "OK");
                        continueWrite = false;
                        Debug.WriteLine("Total amount of hours is above 24 for this date");
                    }
                    else
                    {
                        databaseItems.Add(new DieselDatabase
                        {
                            EntryDate = dateSelected,
                            ID = 0,
                            LeisureHours = Convert.ToInt32(sfDiesEntry.Value),
                            PropHours = Convert.ToInt32(sfPropEntry.Value),
                            DieselRefill = 0
                        });
                        continueWrite = true;
                    }
                    break;

                //Otherwise add the inputted hours onto the database entry
                case 1:
                    //Check hours for this date are not above 24 hours 
                    if (
                        sfDiesEntry.Value + sfPropEntry.Value > 24
                    )
                    {
                        await DisplayAlert("Alert", "Total amount of hours is above 24 for this date", "OK");
                        continueWrite = false;
                        Debug.WriteLine("Total amount of hours is above 24 for this date");
                    }
                    else
                    {
                        databaseItems[0].LeisureHours = Convert.ToInt32(sfDiesEntry.Value);
                        databaseItems[0].PropHours = Convert.ToInt32(sfPropEntry.Value);
                        continueWrite = true;
                    }
                    break;

                //Should never see this, it is an error
                default:
                    Debug.WriteLine("More than 1 item for this date in the database");
                    continueWrite = false;
                    break;
            }

            if (continueWrite)
            {
                //Write the changes and leave the view
                await database.SaveItemAsync(databaseItems[0]);
                await DisplayAlert("Alert", "This date has now been updated", "Ok");
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    }

    async void OnDeleteDateClicked(object? sender, EventArgs e)
    {
        try
        {
            //Confirm user wants to delete
            bool answer = await DisplayAlert("Confirm Delete", "Are you sure you want to delete the hours for this date?", "Yes", "No");
            if (!answer)
                return;

            //Get users date from the form
            DateTime selectedDate = (DateTime)HoursDatePicker.SelectedDate;
            DateTime date = selectedDate;
            var dateSelected = date.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                // If database has no records for that date, inform user
                case 0:
                    await DisplayAlert("Alert", "There is nothing to delete for this date in the database", "OK");
                    sfDiesEntry.Value = null;
                    sfPropEntry.Value = null;
                    break;
                //Delete the hours for this date by setting them to 0
                case 1:
                    databaseItems[0].LeisureHours = 0;
                    databaseItems[0].PropHours = 0;
                    await database.SaveItemAsync(databaseItems[0]);
                    sfDiesEntry.Value = null;
                    sfPropEntry.Value = null;
                    break;
                //Should never see this, it is an error
                default:
                    Debug.WriteLine("More than 1 item found for this date");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    }
}