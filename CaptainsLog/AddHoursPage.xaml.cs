using CaptainsLog.DatabaseClasses;
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
    }
    async void OnAddHoursClicked(object? sender, EventArgs e)
    {
        try
        {
            //check user has entered at least one value
            if (String.IsNullOrEmpty(DiesEntry.Text) && String.IsNullOrEmpty(PropEntry.Text))
            {
                await DisplayAlert("Alert", "Please enter hours for either Diesel or Leisure", "OK");
                return;
            }

            //Set any empty entries to 0
            if (String.IsNullOrEmpty(DiesEntry.Text))
                DiesEntry.Text = "0";
            //set any empty entries to 0
            if (String.IsNullOrEmpty(PropEntry.Text))
                PropEntry.Text = "0";

            //Confirm user wants to add
            bool answer = await DisplayAlert("Confirm Add", "Are you sure you want to add these hours?", "Yes", "No");
            if (!answer)
                return;

            //Get users date from the form
            var dateSelected = DateEntry.Date.ToString("yyyy-MM-dd");
            var continueWrite = true;

            //Check and get any entries from the database for this date, there should always be 1. 
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                // If database has no records for that date, insert a new record
                case 0:
                    //Check hours for this date are not above 24 hours
                    if (Convert.ToInt32(DiesEntry.Text) + Convert.ToInt32(PropEntry.Text) > 24)
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
                            LeisureHours = Convert.ToInt32(DiesEntry.Text),
                            PropHours = Convert.ToInt32(PropEntry.Text),
                            DieselRefill = 0
                        });
                        continueWrite = true;
                    }
                    break;

                //Otherwise add the inputted hours onto the database entry
                case 1:
                    //Check hours for this date are not above 24 hours 
                    if (
                        databaseItems[0].LeisureHours + Convert.ToInt32(DiesEntry.Text) +
                        databaseItems[0].PropHours + Convert.ToInt32(PropEntry.Text) > 24
                    )
                    {
                        await DisplayAlert("Alert", "Total amount of hours is above 24 for this date", "OK");
                        continueWrite = false;
                        Debug.WriteLine("Total amount of hours is above 24 for this date");
                    }
                    else
                    {
                        databaseItems[0].LeisureHours += Convert.ToInt32(DiesEntry.Text);
                        databaseItems[0].PropHours += Convert.ToInt32(PropEntry.Text);
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
                await Navigation.PopAsync();
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
            var dateSelected = DateEntry.Date.ToString("yyyy-MM-dd");

            //Check and get any entries from the database for this date
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where EntryDate = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                // If database has no records for that date, inform user
                case 0:
                    await DisplayAlert("Alert", "There is nothing to delete for this date", "OK");
                    break;
                //Delete the hours for this date by setting them to 0
                case 1:
                    databaseItems[0].LeisureHours = 0;
                    databaseItems[0].PropHours = 0;
                    await database.SaveItemAsync(databaseItems[0]);
                    DiesEntry.Text = "";
                    PropEntry.Text = "";
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