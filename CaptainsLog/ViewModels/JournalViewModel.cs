using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui; // For Application reference if needed
using Microsoft.Maui.Storage; // For MediaPicker
using Microsoft.Maui.Controls;
using CaptainsLog.JournalPages; // For DisplayAlert

namespace CaptainsLog.ViewModels
{
    public partial class JournalViewModel : BaseViewModel
    {
        private readonly JournalSQLTools _journalSQLTools;

        [ObservableProperty]
        public ObservableCollection<JournalItem>? journalItems = new();

        private static int CurrentID = 0;

        // Journal Entry Properties
        [ObservableProperty]
        public string journalPicturePath = "photodefault.jpg";
        [ObservableProperty]
        public DateTime journalEntryDate = DateTime.Now;
        [ObservableProperty]
        public string journalTitle = string.Empty;
        [ObservableProperty]
        public string journalContent = string.Empty;
        [ObservableProperty]
        public string journalLocation = string.Empty;

        public JournalViewModel(JournalSQLTools journalJSONTools)
        {
            _journalSQLTools = journalJSONTools;
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
                ID = 0, // ID will be set by the database
                PicturePath = JournalPicturePath,
                EntryDate = entryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), // Convert DateTime to string
                Title = JournalTitle,
                Content = JournalContent,
                Location = JournalLocation
            };
            await _journalSQLTools.SaveItemAsync(newEntry);
            JournalItems?.Add(newEntry);
            // Clear input fields after saving
            JournalPicturePath = "photodefault.jpg";
            JournalEntryDate = DateTime.Now;
            JournalTitle = string.Empty;
            JournalContent = string.Empty;
            JournalLocation = string.Empty;
        }

        // Add this method to JournalViewModel to resolve CS0103 for ShowAlertAsync
        private async Task ShowAlertAsync(string title, string message, string cancel)
        {
            // If using MAUI, you can use Application.Current.MainPage.DisplayAlert
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        // PSEUDOCODE / PLAN:
        // 1. Create an async command method named `OpenJournalEntryPageAsync` with [RelayCommand].
        // 2. Instantiate a new `JournalEntryPage` page.
        // 3. Optionally set the page's BindingContext (use `this` so the page can bind to this view model).
        // 4. Attempt navigation:
        //    - If Application.Current.MainPage has a Navigation stack, use PushAsync(page).
        //    - Else if running with Shell, try Shell.Current.GoToAsync with the route name as a fallback.
        //    - If no navigation available, show an alert explaining the error.
        // 5. Wrap navigation in try/catch and report any exceptions via ShowAlertAsync.
        // 6. Mark method with [RelayCommand] so the UI can bind to `OpenJournalEntryPageCommand`.
        [RelayCommand]
        private async Task OpenJournalEntryPageAsync()
        {
            try
            {
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

    }
}
