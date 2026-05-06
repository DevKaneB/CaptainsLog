using CaptainsLog.DatabaseClasses;
using CaptainsLog.DatabaseClasses.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly ProfileJSONTools _profileJSONTools;
        private readonly JournalSQLTools _journalSQLTools;
        private readonly DieselDatabaseMethods _dieselDataSQLTools;  
        private readonly ExpensesSQLTools _expensesDataSQLTools;

        public SettingsViewModel()
        {
            _profileJSONTools = new ProfileJSONTools();
            _journalSQLTools = new JournalSQLTools();
            _dieselDataSQLTools = new DieselDatabaseMethods();
            _expensesDataSQLTools = new ExpensesSQLTools();
        }

        [RelayCommand]
        private async Task BackButtonClicked()
        {
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var navigation = mainWindow?.Page?.Navigation;
            if (navigation != null)
            {
                await navigation.PopAsync();
            }
        }

        //Delete Profile Data and stored image file path (asks for confirmation)
        [RelayCommand]
        private async Task Del_ProfileData()
        {
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var page = mainWindow?.Page;
            if (page == null)
                return;

            bool confirm = await page.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete the profile data and profile image stored within this apps data? This action cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            bool anyDeleted = false;

            // Check for the existence of the profile JSON file and delete it if it exists
            var JSONPath = Constants.ProfileJSONDatabasePath;
            if (!string.IsNullOrWhiteSpace(JSONPath) && File.Exists(JSONPath))
            {
                try
                {
                    File.Delete(JSONPath);
                    anyDeleted = true;
                }
                catch (Exception ex)
                {
                    await page.DisplayAlert("Error", $"Error deleting profile JSON file: {ex.Message}", "OK");
                }
            }

            // Delete the stored profile image file if it exists
            var imagePath = Constants.ProfileImageFilename;
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    File.Delete(imagePath);
                    anyDeleted = true;
                }
                catch (Exception ex)
                {
                    await page.DisplayAlert("Error", $"Error deleting profile image file: {ex.Message}", "OK");
                }
            }

            if (anyDeleted)
            {
                await page.DisplayAlert("Deleted", "Selected profile files were deleted.", "OK");
            }
            else
            {
                await page.DisplayAlert("Nothing Found", "No profile data or image file was found to delete.", "OK");
            }
        }

        [RelayCommand]
        private async Task Del_DieselData()
        {
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var page = mainWindow?.Page;
            if (page == null)
                return;
            bool confirm = await page.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete the Diesel Data? This action cannot be undone.",
                "Delete",
                "Cancel");
            if (!confirm)
                return;
            try
            {
                await _dieselDataSQLTools.DeleteAllItemsAsync();
                await page.DisplayAlert("Deleted", "Diesel Data  was deleted.", "OK");
            }
            catch (Exception ex)
            {
                await page.DisplayAlert("Error", $"Error deleting Diesel Data: {ex.Message}", "OK");
            }
            
        }

        [RelayCommand]
        private async Task Del_ExpensesData()
        {
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var page = mainWindow?.Page;
            if (page == null)
                return;
            bool confirm = await page.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete the Expenses Data? This action cannot be undone.",
                "Delete",
                "Cancel");
            if (!confirm)
                return;
            try
            {
                await _expensesDataSQLTools.DeleteAllItemsAsync();
                await page.DisplayAlert("Deleted", "Expenses Data was deleted.", "OK");
            }
            catch (Exception ex)
            {
                await page.DisplayAlert("Error", $"Error deleting Diesel Data: {ex.Message}", "OK");
            }

        }

        [RelayCommand]
        private async Task Del_JournalData()
        {
            //Delete journal data by deleting the table within the SQL database (asks for confirmation)
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var page = mainWindow?.Page;
            if (page == null)
                return;
            bool confirm = await page.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete the Journal data? This action cannot be undone.",
                "Delete",
                "Cancel");
            if (!confirm)
                return;

            bool dbDeleted = false;
            bool photosDeleted = false;

            try
            {
                await _journalSQLTools.DeleteAllItemsAsync();
                dbDeleted = true;
            }
            catch (Exception ex)
            {
                await page.DisplayAlert("Error", $"Error deleting journal data: {ex.Message}", "OK");
            }

            // Attempt to delete 'journalphotos' folder inside AppDataDirectory
            try
            {
                var appDataDir = FileSystem.AppDataDirectory;
                if (!string.IsNullOrWhiteSpace(appDataDir))
                {
                    var journalPhotosDir = Path.Combine(appDataDir, "journalphotos");
                    if (Directory.Exists(journalPhotosDir))
                    {
                        Directory.Delete(journalPhotosDir, true);
                        photosDeleted = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await page.DisplayAlert("Error", $"Error deleting journal photos folder: {ex.Message}", "OK");
            }

            if (dbDeleted || photosDeleted)
            {
                string msg;
                if (dbDeleted && photosDeleted)
                    msg = "Journal data and journal photos were deleted from the app.";
                else if (dbDeleted)
                    msg = "Journal data was deleted.";
                else
                    msg = "journalphotos folder was deleted.";

                await page.DisplayAlert("Deleted", msg, "OK");
            }
            else
            {
                await page.DisplayAlert("Nothing Found", "No journal data or journal photos was found to delete.", "OK");
            }
        }
    }
}
