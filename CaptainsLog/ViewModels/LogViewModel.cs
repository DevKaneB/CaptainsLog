using CaptainsLog.DatabaseClasses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CaptainsLog.ViewModels
{
    public partial class LogViewModel : BaseViewModel
    {
        private readonly DieselDatabaseMethods _databaseClient;

        public LogViewModel(DieselDatabaseMethods DatabaseClient)
        {
            _databaseClient = DatabaseClient;
        }

        // Fix for MVVMTK0045: Use a partial property instead of a field for [ObservableProperty]
        [ObservableProperty]
        ObservableCollection<DieselDatabase> databaseItems = new();

        [RelayCommand]
        public async Task LoadDatabaseItemsAsync()
        {
            var items = await _databaseClient.GetItemsViaQueryAsync("Select * from DieselDatabase Order By EntryDate DESC");
            DatabaseItems = new ObservableCollection<DieselDatabase>(items);
        }
    }
}
