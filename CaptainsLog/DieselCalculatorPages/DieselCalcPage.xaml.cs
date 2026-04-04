using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using CaptainsLog.ViewModels;
using System.Diagnostics;

namespace CaptainsLog;

public partial class DieselCalcPage : ContentPage
{
    private DieselDatabaseMethods database;
    private List<DieselDatabase>? databaseItems;

    public DieselCalcPage()
    {
        InitializeComponent();
        database = new DieselDatabaseMethods();
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
        await DisplayAlert("Help", "Diesel Calculator lets you view your tax declaration and what you use per hour in litres. Please add engine hours and/or diesel refills to view filters.", "OK");
        return;
    }

    async void OnAddUpdateHoursClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new AddHoursPage());
        }
    }

    async void OnLastRefillClicked(object? sender, EventArgs e)
    {
        try
        {
            //Are there any diesel Refill entries in the database?
            var RefillItems =
                await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where DieselRefill != '0'");

            if (RefillItems.Count == 0)
            {
                await DisplayAlert("Alert", "No entries found", "OK");
                return;

            }


            //Load all database entries
            databaseItems = await database.GetItemsViaQueryAsync($"Select * from DieselDatabase");

            //Check if any entries found and show alert if none
            if (databaseItems.Count == 0)
            {
                await DisplayAlert("Alert", "No entries found", "OK");
                return;
            }
            //If entries found, clear list and run query to get sum of hours since last refill
            else
            {
                databaseItems.Clear();

                databaseItems =
                       await database.GetItemsViaQueryAsync($"SELECT SUM(LeisureHours) AS LeisureHours, SUM(PropHours) AS PropHours, SUM(DieselRefill) AS DieselRefill FROM DieselDatabase WHERE EntryDate > (SELECT EntryDate FROM DieselDatabase WHERE DieselRefill != '0' ORDER BY EntryDate DESC LIMIT 1)");
                
                switch (databaseItems.Count)
                {
                    //Calculate and display percentages
                    case 1:

                        if (databaseItems[0].LeisureHours == 0 && databaseItems[0].PropHours == 0)
                        {
                            await DisplayAlert("Alert", "No hours recorded since last diesel refill", "OK");
                            return;
                        }

                        var dieselRefillItems =
                        await database.GetItemsViaQueryAsync($"SELECT DieselRefill FROM DieselDatabase WHERE EntryDate = (SELECT EntryDate FROM DieselDatabase WHERE DieselRefill != '0' ORDER BY EntryDate DESC LIMIT 1)");


                        var item = databaseItems[0];
                        float DHours = item.LeisureHours;
                        float PHours = item.PropHours;
                        float DieselRefill = dieselRefillItems[0].DieselRefill;
                        var DieselPercent = Math.Round((DHours / (PHours + DHours)) * 100, 0);
                        var PropPercent = Math.Round((PHours / (PHours + DHours)) * 100, 0);
                        var DieselPerLitre = Math.Round(DieselRefill / (PHours + DHours), 2);
                        PropHrsLbl.Text = $"{PropPercent}%";
                        DiesHrsLbl.Text = $"{DieselPercent}%";
                        LitrePHourLbl.Text = $"{DieselPerLitre} Litres/Hour";

                        dieselRefillItems.Clear();
                        break;
                    //error condition - multiple records found
                    default:
                        await DisplayAlert("Alert", "Unexpected number of entries found", "OK");
                        break;
                }

            }

            databaseItems.Clear();
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    async void OnLastThirtyDaysClicked(object? sender, EventArgs e)
    {
        try
        {
            //Load all database entries
            databaseItems = await database.GetItemsViaQueryAsync($"Select * from DieselDatabase");
            //Check if any entries found and show alert if none
            if (databaseItems.Count == 0)
            {
                await DisplayAlert("Alert", "No entries found", "OK");
                return;
            }
            //If entries found, clear list and run query to get sum of hours in last 30 days
            else
            {
                databaseItems.Clear();

                databaseItems =
                       await database.GetItemsViaQueryAsync($"SELECT SUM(LeisureHours) AS LeisureHours,SUM(PropHours) AS PropHours from DieselDatabase where EntryDate > date('now','-30 day')");

                switch (databaseItems.Count)
                {
                    //load and display percentages
                    case 1:
                        //Check if any hours recorded
                        if (databaseItems[0].LeisureHours == 0 && databaseItems[0].PropHours == 0)
                        {
                            await DisplayAlert("Alert", "No hours recorded in the last 30 days", "OK");
                            return;
                        }
                        var item = databaseItems[0];
                        float DHours = item.LeisureHours;
                        float PHours = item.PropHours;
                        var DieselPercent = Math.Round((DHours / (PHours + DHours)) * 100, 0);
                        var PropPercent = Math.Round((PHours / (PHours + DHours)) * 100, 0);
                        PropHrsLbl.Text = $"{PropPercent}%";
                        DiesHrsLbl.Text = $"{DieselPercent}%";
                        break;
                    //error condition - multiple records found
                    default:
                        await DisplayAlert("Alert", "Unexpected number of entries found", "OK");
                        break;
                }

            }

            databaseItems.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

    }

    async void OnAllTimeClicked(object? sender, EventArgs e)
    {
        try
        {
            //Load all database entries
            databaseItems = await database.GetItemsViaQueryAsync($"Select * from DieselDatabase");

            //Check if any entries found and show alert if none
            if (databaseItems.Count == 0)
            {
                await DisplayAlert("Alert", "No entries found", "OK");
                return;
            }
            //If entries found, clear list and run query to get sum of all hours
            else
            {
                databaseItems.Clear();

                databaseItems =
                       await database.GetItemsViaQueryAsync($"Select SUM(LeisureHours) AS LeisureHours, SUM(PropHours) AS PropHours From DieselDatabase");

                //Calculate and display percentages
                switch (databaseItems.Count)
                {
                    //a record is found
                    case 1:
                        //Check if any hours recorded
                        if (databaseItems[0].LeisureHours == 0 && databaseItems[0].PropHours == 0)
                        {
                            await DisplayAlert("Alert", "No hours recorded in database", "OK");
                            return;
                        }

                        var item = databaseItems[0];
                        float DHours = item.LeisureHours;
                        float PHours = item.PropHours;
                        var DieselPercent = Math.Round((DHours / (PHours + DHours)) * 100, 0);
                        var PropPercent = Math.Round((PHours / (PHours + DHours)) * 100, 0);
                        PropHrsLbl.Text = $"{PropPercent}%";
                        DiesHrsLbl.Text = $"{DieselPercent}%";
                        break;

                    //error condition - multiple records found
                    default:
                        await DisplayAlert("Alert", "Unexpected number of entries found", "OK");
                        break;
                }

            }

            databaseItems.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    async void OnDieselRefillClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new AddDieselLitresPage());
        }
    }

    async void OnViewHistoryClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new DieselLogPage());
        }
    }
}