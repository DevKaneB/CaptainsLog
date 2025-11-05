using CaptainsLog.DatabaseClasses;

namespace CaptainsLog;

public partial class DieselLogPage : ContentPage
{
    private DieselDatabaseMethods database;
    private List<DieselDatabase>? databaseItems;

    public DieselLogPage()
	{
        InitializeComponent();

        database = new DieselDatabaseMethods();

        //Load Diesel Database into View
        LoadDatabaseItemsAsync();
    }

    private async void LoadDatabaseItemsAsync()
    {
        databaseItems = await database.GetItemsViaQueryAsync("Select * from DieselDatabase Order By EntryDate DESC");
        listDatabaseItems.ItemsSource = databaseItems;
    }

    async void OnDeleteRowClicked(object? sender, EventArgs e)
    {
        
    }


}