using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using Syncfusion.Maui.Picker;
using System.Diagnostics;

namespace CaptainsLog;

[QueryProperty("EntryDate","entryDate")]
public partial class AddHoursPage : ContentPage
{
    private DieselDatabaseMethods database;
    private List<DieselDatabase>? databaseItems;

    public AddHoursPage()
	{
		InitializeComponent();
        database = new DieselDatabaseMethods();
        _ = LoadDateHours();
    }

    // This property is set when navigating via Shell with a query parameter e.g. "AddHoursPage?entryDate=2026-05-14"
    public string EntryDate
    {
        set
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(value) && DateTime.TryParse(value, out var parsed))
                {
                    // Ensure the DatePicker exists and set its SelectedDate
                        if (HoursDatePicker != null)
                        {
                            HoursDatePicker.SelectedDate = parsed;
                            // After setting date, load associated data
                            _ = LoadDateHours();
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
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

    async Task LoadDateHours()
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
                    sfDiesMinEntry.Value = null;
                    sfPropMinEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesEntry.Value = databaseItems[0].LeisureHours;
                    sfDiesMinEntry.Value = databaseItems[0].LeisureMinutes;
                    sfPropEntry.Value = databaseItems[0].PropHours;
                    sfPropMinEntry.Value = databaseItems[0].PropMinutes;
                    
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
                    sfDiesMinEntry.Value = null;
                    sfPropMinEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesEntry.Value = databaseItems[0].LeisureHours;
                    sfDiesMinEntry.Value = databaseItems[0].LeisureMinutes;
                    sfPropEntry.Value = databaseItems[0].PropHours;
                    sfPropMinEntry.Value = databaseItems[0].PropMinutes;
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
                    sfDiesMinEntry.Value = null;
                    sfPropMinEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesEntry.Value = databaseItems[0].LeisureHours;
                    sfDiesMinEntry.Value = databaseItems[0].LeisureMinutes;
                    sfPropEntry.Value = databaseItems[0].PropHours;
                    sfPropMinEntry.Value = databaseItems[0].PropMinutes;
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
                    if ((sfDiesEntry.Value ?? 0) == databaseItems[0].LeisureHours &&
                        (sfPropEntry.Value ?? 0) == databaseItems[0].PropHours &&
                        (sfPropMinEntry.Value ?? 0 ) == databaseItems[0].PropMinutes &&
                        (sfDiesMinEntry.Value ?? 0) == databaseItems[0].LeisureMinutes)
                    {
                        await DisplayAlert("Alert", "No changes detected to the hours or minutes for this date", "OK");
                        return;
                    }
                    break;
                //Should never see this, it is an error
                default:
                    Debug.WriteLine($"More than 1 item found for this date: {dateSelected}");
                    break;
            }

            //check user has entered at least one value
            if ((sfDiesEntry.Value is null or < 1) && (sfPropEntry.Value is null or < 1) && (sfDiesMinEntry.Value is null or < 1) && (sfPropMinEntry.Value is null or < 1))
            {
                await DisplayAlert("Alert", "Please enter hours or minutes for either Diesel or Leisure", "OK");
                return;
            }

            //Set any empty entries to 0
            if (sfDiesEntry.Value is null or < 1)
                sfDiesEntry.Value = 0;
            //set any empty entries to 0
            if (sfPropEntry.Value is null or < 1)
                sfPropEntry.Value = 0;
            if (sfDiesMinEntry.Value is null or < 1)
                sfDiesMinEntry.Value = 0;
            if (sfPropMinEntry.Value is null or < 1)
                sfPropMinEntry.Value = 0;

            if (sfDiesEntry.Value + sfPropEntry.Value + sfDiesMinEntry.Value + sfPropMinEntry.Value == 0)
            {
                await DisplayAlert("Alert", "You are about to enter no hours, please press the tick to apply the hours or minutes before saving", "OK");
                return;
            }

            //Confirm user wants to add
            bool answer = await DisplayAlert("Confirm Add", "Are you sure you want to add these hours and minutes?", "Yes", "No");
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
                    {
                        int diesH = Convert.ToInt32(sfDiesEntry.Value ?? 0);
                        int propH = Convert.ToInt32(sfPropEntry.Value ?? 0);
                        int diesM = Convert.ToInt32(sfDiesMinEntry.Value ?? 0);
                        int propM = Convert.ToInt32(sfPropMinEntry.Value ?? 0);
                        int totalMinutes = diesH * 60 + diesM + propH * 60 + propM;

                        //Check total combined hours/minutes for this date are not above 24 hours
                        if (totalMinutes > 24 * 60)
                        {
                            await DisplayAlert("Alert", "Total combined hours and minutes exceed 24 hours for this date", "OK");
                            continueWrite = false;
                            Debug.WriteLine("Total combined hours and minutes exceed 24 for this date");
                        }
                        else
                        {
                            databaseItems.Add(new DieselDatabase
                            {
                                EntryDate = dateSelected,
                                ID = 0,
                                LeisureHours = Convert.ToInt32(sfDiesEntry.Value),
                                LeisureMinutes = Convert.ToInt32(sfDiesMinEntry.Value),
                                PropHours = Convert.ToInt32(sfPropEntry.Value),
                                PropMinutes = Convert.ToInt32(sfPropMinEntry.Value),
                                DieselRefill = 0
                            });
                            continueWrite = true;
                        }
                    }
                    break;

                //Otherwise add the inputted hours onto the database entry
                case 1:
                    {
                        int diesH = Convert.ToInt32(sfDiesEntry.Value ?? 0);
                        int propH = Convert.ToInt32(sfPropEntry.Value ?? 0);
                        int diesM = Convert.ToInt32(sfDiesMinEntry.Value ?? 0);
                        int propM = Convert.ToInt32(sfPropMinEntry.Value ?? 0);
                        int totalMinutes = diesH * 60 + diesM + propH * 60 + propM;

                        //Check total combined hours/minutes for this date are not above 24 hours 
                        if (totalMinutes > 24 * 60)
                        {
                            await DisplayAlert("Alert", "Total combined hours and minutes exceed 24 hours for this date", "OK");
                            continueWrite = false;
                            Debug.WriteLine("Total combined hours and minutes exceed 24 for this date");
                        }
                        else
                        {
                            databaseItems[0].LeisureHours = Convert.ToInt32(sfDiesEntry.Value);
                            databaseItems[0].PropHours = Convert.ToInt32(sfPropEntry.Value);
                            databaseItems[0].LeisureMinutes = Convert.ToInt32(sfDiesMinEntry.Value);
                            databaseItems[0].PropMinutes = Convert.ToInt32(sfPropMinEntry.Value);
                            continueWrite = true;
                        }
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
                    sfDiesMinEntry.Value = null;
                    sfPropEntry.Value = null;
                    sfPropMinEntry.Value = null;
                    break;
                //Delete the hours for this date by setting them to 0
                case 1:
                    databaseItems[0].LeisureHours = 0;
                    databaseItems[0].PropHours = 0;
                    databaseItems[0].LeisureMinutes = 0;
                    databaseItems[0].PropMinutes = 0;
                    await database.SaveItemAsync(databaseItems[0]);
                    sfDiesEntry.Value = null;
                    sfDiesMinEntry.Value = null;
                    sfPropEntry.Value = null;
                    sfPropMinEntry.Value = null;
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