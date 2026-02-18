using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using CaptainsLog.JournalPages; // For DisplayAlert
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace CaptainsLog.ViewModels
{
    public partial class JournalViewModel : BaseViewModel
    {
        private readonly JournalSQLTools _journalSQLTools;

        private int CurrentID = 0;

        [ObservableProperty]
        public ObservableCollection<JournalItem>? journalItems = new();

        // Journal Entry Properties
        [ObservableProperty]
        public string journalPicturePath = "photodefault.jpg";
        public string SavedJournalPicturePath { get; set; }
        [ObservableProperty]
        public DateTime journalEntryDate = DateTime.Now;
        [ObservableProperty]
        public string journalTitle = string.Empty;
        [ObservableProperty]
        public string journalContent = string.Empty;
        [ObservableProperty]
        public string journalLocation = string.Empty;

        // Journal Entry Properties
        [ObservableProperty]
        public string journalVPicturePath = "photodefault.jpg";
        [ObservableProperty]
        public DateTime journalVEntryDate = DateTime.Now;
        [ObservableProperty]
        public string journalVTitle = string.Empty;
        [ObservableProperty]
        public string journalVContent = string.Empty;
        [ObservableProperty]
        public string journalVLocation = string.Empty;

        public JournalViewModel(JournalSQLTools journalJSONTools)
        {
            _journalSQLTools = journalJSONTools;
        }

        [RelayCommand]
        public async Task OnBackButtonClicked()
        {
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var navigation = mainWindow?.Page?.Navigation;
            if (navigation != null)
            {
                await navigation.PopAsync();
            }
        }

        [RelayCommand]
        public async Task OnHelpButtonClicked()
        {
            // Get the current page safely
            var page = Microsoft.Maui.Controls.Application.Current?.MainPage;

            // If no page is available, cancel the delete to avoid throwing
            if (page == null)
                return;

            // Ask the user to confirm deletion
            await page.DisplayAlert(
                "Help",
                "Add a new page to your journal. Pick a date, select a picture, add some details about your day and press save. Selecting a day with data will let you edit that day.",
                "Ok");
            return;
        }
        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a journal entry photo"
                });

                if (result == null)
                    return; // user canceled

                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                string folderName = "journalphotos";
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                string fileName = $"{timestamp}.png";
                string filePath = Path.Combine(FileSystem.AppDataDirectory, folderName, fileName);
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Overwrite existing file
                using (var fileStream = File.OpenWrite(filePath))
                {
                    ms.Position = 0;
                    await ms.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }

                // --- IMPORTANT: update the bound model so UI will show the image ---
                if (JournalPicturePath != null)
                {
                    // set to the local file path (MAUI Image accepts local file path strings)
                    JournalPicturePath = filePath;
                    SavedJournalPicturePath = Path.Combine(folderName, fileName);
                }
            }
            catch (PermissionException)
            {
                await ShowAlertAsync("Permissions", "Permission to access photos was denied.", "OK");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Unable to pick photo: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SaveJournalEntryAsync()
        {
            if (string.IsNullOrWhiteSpace(JournalTitle) || string.IsNullOrWhiteSpace(JournalContent))
            {
                await ShowAlertAsync("Validation Error", "Title and Content cannot be empty.", "OK");
                return;
            }

            // Confirm save with the user before proceeding
            var confirmSave = await Application.Current.MainPage.DisplayAlert(
                "Confirm Save",
                "Do you want to save this journal entry?",
                "Yes",
                "No");

            if (!confirmSave)
                return;

            // Convert JournalEntryDate to a date-only value formatted as "yyyy-MM-dd",
            // then parse back to DateTime to ensure the time component is 00:00:00.
            var entryDateString = JournalEntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var entryDate = DateTime.ParseExact(entryDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var newEntry = new JournalItem
            {
                ID = CurrentID, // ID will be set by the database
                PicturePath = SavedJournalPicturePath,
                EntryDate = entryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), // Convert DateTime to string
                Title = JournalTitle,
                Content = JournalContent,
                Location = JournalLocation
            };
            await _journalSQLTools.SaveItemAsync(newEntry);
        }

        [RelayCommand]
        private async Task OpenJournalEntryPageAsync()
        {
            try
            {
                JournalItems = new ObservableCollection<JournalItem>(await _journalSQLTools.GetItemsAsync());

                if (JournalItems == null || JournalItems.Count == 0)
                {
                    await ShowAlertAsync("No Entries", "There are no journal entries to display.", "OK");
                    return;
                }

                // Create the page and supply this view model as its BindingContext
                var page = new JournalEntryPage
                {
                    BindingContext = this
                };

                // Prefer pushing onto the existing Navigation stack if available
                if (Application.Current?.MainPage?.Navigation != null)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(page);
                    return;
                }

                // If using Shell and a route is registered, try Shell navigation as a fallback
                if (Shell.Current != null)
                {
                    // Attempt to go to a route with the page's type name (route must be registered elsewhere)
                    await Shell.Current.GoToAsync(nameof(JournalEntryPage));
                    return;
                }

                // If neither navigation nor shell is available, inform the user
                await ShowAlertAsync("Navigation Error", "No navigation context available to open the journal entry page.", "OK");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Navigation Error", $"Failed to open JournalEntryPage: {ex.Message}", "OK");
            }
        }

        // Add this method to JournalViewModel to resolve CS0103 for ShowAlertAsync
        private async Task ShowAlertAsync(string title, string message, string cancel)
        {
            // If using MAUI, you can use Application.Current.MainPage.DisplayAlert
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        //Load the diary View Pages
        public async Task LoadEntryPageData()
        {
            try 
            { 
                var ReadEntry = await _journalSQLTools.GetItemsViaQueryAsync("SELECT * FROM JournalItem WHERE EntryDate  <= DATE('now') ORDER BY EntryDate DESC LIMIT 1");
                var photopath = "photodefault.jpg";
                var LocationString = string.Empty;

                if (ReadEntry != null && ReadEntry.Count != 0)
                {
                    if (ReadEntry[0].PicturePath != null)
                    {
                        photopath = Path.Combine(FileSystem.AppDataDirectory, ReadEntry[0].PicturePath);
                    }
                    if (ReadEntry[0].Location != null)
                    {
                        LocationString = ReadEntry[0].Location;
                    }

                    CurrentID = ReadEntry[0].ID;
                    // Resolve and assign an image path usable by MAUI Image
                    JournalVPicturePath = photopath;
                    JournalVEntryDate = DateTime.ParseExact(ReadEntry[0].EntryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    JournalVTitle = ReadEntry[0].Title;
                    JournalVContent = ReadEntry[0].Content;
                    JournalVLocation = LocationString;
                }        
            }
            catch (Exception ex)
            {
                CurrentID = 0;
            }
        }
        // This method is automatically called whenever the date changes
        async partial void OnJournalEntryDateChanged(DateTime value)
        {
            try
            {
                // Handle exceptions as needed

                // Convert JournalEntryDate to a date-only value formatted as "yyyy-MM-dd",
                // then parse back to DateTime to ensure the time component is 00:00:00.
                var entryDateString = JournalEntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var entryDate = DateTime.ParseExact(entryDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var SQLDate = entryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                var ReadEntry = await _journalSQLTools.GetItemsViaQueryAsync($"SELECT * FROM JournalItem WHERE EntryDate = '{SQLDate}' LIMIT 1");

                var photopath = "photodefault.jpg";
                var LocationString = string.Empty;

                if (ReadEntry != null && ReadEntry.Count != 0)
                {
                    //Catch Null Value
                    if (ReadEntry[0].PicturePath != null)
                    {
                        photopath = Path.Combine(FileSystem.AppDataDirectory, ReadEntry[0].PicturePath);
                    }

                    //Catch Null Value
                    if (ReadEntry[0].Location != null)
                    {
                        LocationString = ReadEntry[0].Location;
                    }

                    CurrentID = ReadEntry[0].ID;
                    // Resolve and assign an image path usable by MAUI Image
                    JournalPicturePath = photopath;
                    SavedJournalPicturePath = ReadEntry[0].PicturePath;
                    JournalTitle = ReadEntry[0].Title;
                    JournalContent = ReadEntry[0].Content;
                    JournalLocation = LocationString;
                }
                else
                {
                    //Clear records if date changed 
                    CurrentID = 0;
                    JournalPicturePath = "photodefault.jpg";
                    SavedJournalPicturePath = "photodefault.jpg";
                    JournalTitle = string.Empty;
                    JournalContent = string.Empty;
                    JournalLocation = LocationString;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading journal entry for date {value}: {ex.Message}");
            }
        }
        [RelayCommand]
        private async Task NextPage()
        {
            try
            {
                var ReadEntry = await _journalSQLTools.GetItemsViaQueryAsync($"SELECT * FROM JournalItem WHERE EntryDate  > " +
                                                                             $"( " +
                                                                             $"select EntryDate From JournalItem Where ID = {CurrentID}" +
                                                                             $") " +
                                                                             $"ORDER BY EntryDate ASC LIMIT 1");

                var photopath = "photodefault.jpg";
                var LocationString = string.Empty;

                if (ReadEntry != null && ReadEntry.Count != 0)
                {
                    if (ReadEntry[0].PicturePath != null)
                    {
                        photopath = Path.Combine(FileSystem.AppDataDirectory, ReadEntry[0].PicturePath);
                    }
                    if (ReadEntry[0].Location != null)
                    {
                        LocationString = ReadEntry[0].Location;
                    }

                    CurrentID = ReadEntry[0].ID;
                    // Resolve and assign an image path usable by MAUI Image
                    JournalVPicturePath = Path.Combine(FileSystem.AppDataDirectory, ReadEntry[0].PicturePath);
                    JournalVEntryDate = DateTime.ParseExact(ReadEntry[0].EntryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    JournalVTitle = ReadEntry[0].Title;
                    JournalVContent = ReadEntry[0].Content;
                    JournalVLocation = ReadEntry[0].Location;
                } else
                {
                    await ShowAlertAsync("End of Entries", "There are no more journal entries to show.", "OK");
                }
            }
            catch (Exception ex)
            {
                CurrentID = 0;
            }
        }
        [RelayCommand]
        private async Task PrevPage()
        {
            try
            {
                var ReadEntry = await _journalSQLTools.GetItemsViaQueryAsync($"SELECT * FROM JournalItem WHERE EntryDate <" +
                                                                             $"( " +
                                                                             $"select EntryDate From JournalItem Where ID = {CurrentID}" +
                                                                             $") " +
                                                                             $"ORDER BY EntryDate DESC LIMIT 1");
                if (ReadEntry != null && ReadEntry.Count != 0)
                {
                    CurrentID = ReadEntry[0].ID;
                    // Resolve and assign an image path usable by MAUI Image
                    JournalVPicturePath = Path.Combine(FileSystem.AppDataDirectory, ReadEntry[0].PicturePath);
                    JournalVEntryDate = DateTime.ParseExact(ReadEntry[0].EntryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    JournalVTitle = ReadEntry[0].Title;
                    JournalVContent = ReadEntry[0].Content;
                    JournalVLocation = ReadEntry[0].Location;
                }
                else
                {
                    await ShowAlertAsync("End of Entries", "There are no more journal entries to show.", "OK");
                }
            }
            catch (Exception ex)
            {
                CurrentID = 0;
            }
        }
    }
}
