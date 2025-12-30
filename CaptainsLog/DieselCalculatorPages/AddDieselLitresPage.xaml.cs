using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using Syncfusion.Maui.Picker;
using System.Diagnostics;

namespace CaptainsLog;

public partial class AddDieselLitresPage : ContentPage
{

    private DieselDatabaseMethods database;
    private List<DieselDatabase>? databaseItems;

    public AddDieselLitresPage()
	{
		InitializeComponent();
        database = new DieselDatabaseMethods();
        LoadDateLitres();
    }

    async void LoadDateLitres()
    {
        try
        {
            //Get users date from the form
            DateTime date = LitresDatePicker.SelectedDate.Value;
            var dateSelected = date.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");
            switch (databaseItems.Count)
            {
                // If database has no records for that date, clear the entries
                case 0:
                    sfDiesLitreEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesLitreEntry.Value = databaseItems[0].DieselRefill;
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
                    sfDiesLitreEntry.Value = null;
                    break;
                //Populate the entries with the hours from the database
                case 1:
                    sfDiesLitreEntry.Value = databaseItems[0].DieselRefill;
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

    async void OnAddLitresClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sfDiesLitreEntry.Value < 1)
            {
                await DisplayAlert("Alert", "Please enter litres for Diesel", "OK");
                return;
            }

            bool answer = await DisplayAlert("Confirm Add", "Are you sure you want to add these diesel litres?", "Yes", "No");
            if (!answer)
                return;

            //Get users date from the form
            DateTime date = LitresDatePicker.SelectedDate.Value;
            var dateSelected = date.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date, there should always be 1. 
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                // If database has no records for that date, insert a new record
                case 0:

                    databaseItems.Add(new DieselDatabase
                    {
                        EntryDate = dateSelected,
                        ID = 0,
                        DieselRefill = Convert.ToInt32(sfDiesLitreEntry.Value),
                        PropHours = 0,
                        LeisureHours = 0
                    });
                   
                    break;

                //Otherwise add the inputted hours onto the database entry
                case 1:

                    //check if database matches the current entry text
                    if( databaseItems[0].DieselRefill == Convert.ToInt32(sfDiesLitreEntry.Value))
                    {
                        await DisplayAlert("Alert", "The diesel litres you are trying to add are the same as the existing entry for this date. No changes made.", "OK");
                        return;
                    }

                    databaseItems[0].DieselRefill = Convert.ToInt32(sfDiesLitreEntry.Value);
                    break;

                //Should never see this, it is an error
                default:
                    Debug.WriteLine("More than 1 item for this date in the database");
                    break;
            }

            //Write the changes and leave the view
            await database.SaveItemAsync(databaseItems[0]);
            await DisplayAlert("Alert", "This date has now been updated", "Ok");


        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    }

    async void OnDeleteLitresClicked(object? sender, EventArgs e)
    {
    
        try
        {
            bool answer = await DisplayAlert("Confirm Delete", "Are you sure you want to delete the diesel litres for this date?", "Yes", "No");

            if (!answer)
                return;

            //Get users date from the form
            DateTime date = LitresDatePicker.SelectedDate.Value;
            var dateSelected = date.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                // If database has no records for that date, nothing to delete
                case 0:
                    await DisplayAlert("Alert", "There is nothing to delete for this date", "OK");
                    break;
                //Otherwise set the diesel litres to 0
                case 1:
                    databaseItems[0].DieselRefill = 0;
                    await database.SaveItemAsync(databaseItems[0]);
                    sfDiesLitreEntry.Value = null;
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