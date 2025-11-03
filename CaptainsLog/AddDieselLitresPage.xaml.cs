using CaptainsLog.DatabaseClasses;
using System.Diagnostics;

namespace CaptainsLog;

public partial class AddDieselLitresPage : ContentPage
{

    private DieselDatabaseDatabase database;
    private List<DieselDatabase>? databaseItems;

    public AddDieselLitresPage()
	{
		InitializeComponent();
        database = new DieselDatabaseDatabase();
    }

    async void OnAddLitresClicked(object? sender, EventArgs e)
    {
        try
        {
            bool answer = await DisplayAlert("Confirm Add", "Are you sure you want to add these diesel litres?", "Yes", "No");
            if (!answer)
                return;

            //Get users date from the form
            var dateSelected = DateEntry.Date.ToString("yyyy-MM-dd");

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
                        DieselRefill = Convert.ToInt32(DiesLitreEntry.Text),
                        PropHours = 0,
                        LeisureHours = 0
                    });
                   
                    break;

                //Otherwise add the inputted hours onto the database entry
                case 1:
                    
                    databaseItems[0].DieselRefill += Convert.ToInt32(DiesLitreEntry.Text);
                    break;

                //Should never see this, it is an error
                default:
                    Debug.WriteLine("More than 1 item for this date in the database");
                    break;
            }

            //Write the changes and leave the view
            await database.SaveItemAsync(databaseItems[0]);
            await Navigation.PopAsync();
            

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
            var dateSelected = DateEntry.Date.ToString("yyyy-MM-dd");

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
                    DiesLitreEntry.Text = "";
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