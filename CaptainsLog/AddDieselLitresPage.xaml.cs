using CaptainsLog.DatabaseClasses;
using System.Diagnostics;

namespace CaptainsLog;

public partial class AddDieselLitresPage : ContentPage
{

    private TodoItemDatabase database;
    private List<TodoItem> databaseItems;

    public AddDieselLitresPage()
	{
		InitializeComponent();
        database = new TodoItemDatabase();
    }

    async void OnAddLitresClicked(object? sender, EventArgs e)
    {

        //Get users date from the form
        var dateSelected = DateEntry.Date.ToString("dd-MMM-yyyy");

        try
        {

            //Check and get any entries from the database for this date, there should always be 1. 
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from TodoItem where Date = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                // If database has no records for that date, insert a new record
                case 0:

                    databaseItems.Add(new TodoItem
                    {
                        Date = dateSelected,
                        ID = 0,
                        DieselRefill = Convert.ToInt32(DiesLitreEntry.Text),
                        PropHours = 0,
                        DieselHours = 0
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

    async void OnDeleteLitresButton(object? sender, EventArgs e)
    {
        var dateSelected = DateEntry.Date.ToString("dd-MMM-yyyy");

        try
        {
            databaseItems =
                await database.GetItemsViaQueryAsync($"Select * from TodoItem where Date = '{dateSelected}'");

            switch (databaseItems.Count)
            {
                case 0:
                    await DisplayAlert("Alert", "There is nothing to delete for this date", "OK");
                    break;
                case 1:
                    databaseItems[0].DieselRefill = 0;
                    await database.SaveItemAsync(databaseItems[0]);
                    break;
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