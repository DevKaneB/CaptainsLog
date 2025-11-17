using CaptainsLog.DatabaseClasses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CaptainsLog.ViewModels
{
    public partial class LogViewModel : BaseViewModel
    {
        private readonly DieselDatabaseMethods _databaseClient;

        public LogViewModel(DieselDatabaseMethods DatabaseClient)
        {
            _databaseClient = DatabaseClient;

            // initialize the explicit command so XAML compile-time/type-checking sees it
            DeleteEntryAsyncCommand = new AsyncRelayCommand<int>(async id =>
            {
                // Get the current page safely
                var page = Microsoft.Maui.Controls.Application.Current?.MainPage;

                // If no page is available, cancel the delete to avoid throwing
                if (page == null)
                    return;

                // Ask the user to confirm deletion
                var confirmed = await page.DisplayAlert(
                    "Confirm delete",
                    "Are you sure you want to delete this entry?",
                    "Yes",
                    "No");

                // If the user cancels, do nothing
                if (!confirmed)
                    return;

                // Proceed with deletion
                var itemToDelete = await _databaseClient.GetItemAsync(id);
                if (itemToDelete == null)
                    return;

                await _databaseClient.DeleteItemAsync(itemToDelete);
                await LoadDatabaseItemsAsync();
            });

        }

        [ObservableProperty]
        public ObservableCollection<DieselDatabase>? databaseItems = new();


        [RelayCommand]
        public async Task LoadDatabaseItemsAsync()
        {
            var items = await _databaseClient.GetItemsViaQueryAsync("Select * from DieselDatabase Order By EntryDate DESC");
            DatabaseItems = new ObservableCollection<DieselDatabase>(items);
        }


        // Provide an explicit command property that XAML can see at compile-time.
        // We create and wire it in the constructor above.
        public IAsyncRelayCommand<int> DeleteEntryAsyncCommand { get; }
    }
}
