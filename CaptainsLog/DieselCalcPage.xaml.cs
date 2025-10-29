using CaptainsLog.DatabaseClasses;
using System.Diagnostics;

namespace CaptainsLog;

public partial class DieselCalcPage : ContentPage
{
    private TodoItemDatabase database;
    private List<TodoItem>? databaseItems;

    public DieselCalcPage()
    {
        InitializeComponent();
        database = new TodoItemDatabase();
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
            //Get last 30 records from database and sum up the hours
            databaseItems =
                    await database.GetItemsViaQueryAsync($"Select 99 AS ID, SUM(DieselHours) AS DieselHours, SUM(PropHours) AS PropHours, 0 AS DieselRefill, DATETIME('now') AS Date From TodoItem ORDER BY Date DESC LIMIT 30");

            //Check how many records were returned
            switch (databaseItems.Count)
            {
                //No records found
                case 0:
                    await DisplayAlert("Alert", "No entries found", "OK");
                    break;
                //1 record found as expected
                case 1:
                    var item = databaseItems[0];
                    float DHours = item.LeisureHours;
                    float PHours = item.PropHours;
                    var DieselPercent = Math.Round((DHours / (PHours + DHours)) * 100,0);
                    var PropPercent = Math.Round((PHours / (PHours + DHours)) * 100, 0);
                    PropHrsBtn.Text = $"{PropPercent}%";
                    DiesHrsBtn.Text = $"{DieselPercent}%";
                    break;
                //error condition - multiple records found
                default:
                    await DisplayAlert("Alert", "Unexpected number of entries found", "OK");
                    break;
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
            //Get last 30 records from database and sum up the hours
            databaseItems =
                    await database.GetItemsViaQueryAsync($"Select 99 AS ID, SUM(DieselHours) AS DieselHours, SUM(PropHours) AS PropHours, 0 AS DieselRefill, DATETIME('now') AS Date From TodoItem");

            //Check how many records were returned
            switch (databaseItems.Count)
            {
                //No records found
                case 0:
                    await DisplayAlert("Alert", "No entries found", "OK");
                    break;
                //1 record found as expected
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