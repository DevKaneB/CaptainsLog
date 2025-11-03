using CaptainsLog.DatabaseClasses;
using System.Diagnostics;

namespace CaptainsLog;

public partial class DieselCalcPage : ContentPage
{
    private DieselDatabaseDatabase database;
    private List<DieselDatabase>? databaseItems;

    public DieselCalcPage()
    {
        InitializeComponent();
        database = new DieselDatabaseDatabase();
    }

    async void OnPropHrsClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new AddHoursPage());
        }
    }

    async void OnDiesHrsClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new AddHoursPage());
        }
    }

    async void OnLastRefillClicked(object? sender, EventArgs e)
    {

    }

    async void OnLastThirtyDaysClicked(object? sender, EventArgs e)
    {
        try
        {
            databaseItems = await database.GetItemsViaQueryAsync($"Select * from DieselDatabase");

            if (databaseItems.Count == 0)
            {
                await DisplayAlert("Alert", "No entries found", "OK");
                return;
            }
            else
            {
                databaseItems.Clear();

                databaseItems =
                       await database.GetItemsViaQueryAsync($"SELECT SUM(LeisureHours) AS LeisureHours,SUM(PropHours) AS PropHours from DieselDatabase where EntryDate > date('now','-30 day')");

                switch (databaseItems.Count)
                {
                    //No records found
                    case 1:
                        var item = databaseItems[0];
                        float DHours = item.LeisureHours;
                        float PHours = item.PropHours;
                        var DieselPercent = Math.Round((DHours / (PHours + DHours)) * 100, 0);
                        var PropPercent = Math.Round((PHours / (PHours + DHours)) * 100, 0);
                        PropHrsBtn.Text = $"{PropPercent}%";
                        DiesHrsBtn.Text = $"{DieselPercent}%";
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
            databaseItems = await database.GetItemsViaQueryAsync($"Select * from DieselDatabase");

            if (databaseItems.Count == 0)
            {
                await DisplayAlert("Alert", "No entries found", "OK");
                return;
            }
            else
            {
                databaseItems.Clear();

                databaseItems =
                       await database.GetItemsViaQueryAsync($"Select SUM(LeisureHours) AS LeisureHours, SUM(PropHours) AS PropHours From DieselDatabase");

                switch (databaseItems.Count)
                {
                    //No records found
                    case 1:
                        var item = databaseItems[0];
                        float DHours = item.LeisureHours;
                        float PHours = item.PropHours;
                        var DieselPercent = Math.Round((DHours / (PHours + DHours)) * 100, 0);
                        var PropPercent = Math.Round((PHours / (PHours + DHours)) * 100, 0);
                        PropHrsBtn.Text = $"{PropPercent}%";
                        DiesHrsBtn.Text = $"{DieselPercent}%";
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

    }
}