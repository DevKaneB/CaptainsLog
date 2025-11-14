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
                var itemToDelete = await _databaseClient.GetItemAsync(id);
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
