using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using CaptainsLog.BoatProfilePages;
using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;

namespace CaptainsLog.ViewModels
{
    public partial class ProfilePageModel : BaseViewModel
    {
        private readonly ProfileJSONTools _profileJSONTools;
        private DieselDatabaseMethods database;
        private List<DieselDatabase>? databaseItems;


        public ProfilePageModel(ProfileJSONTools profileJSONTools)
        {
            _profileJSONTools = profileJSONTools;
            database = new DieselDatabaseMethods();
        }

        public double ImageMaxHeight =>
            DeviceDisplay.Current.MainDisplayInfo.Height
            / DeviceDisplay.Current.MainDisplayInfo.Density
            / 3;

        [ObservableProperty]
        public ObservableCollection<ProfileItem>? profileItems = new();


        private async Task ShowAlertAsync(string title, string message, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }

        // Fix for CS0136 and CA1826:
        // - CS0136: Avoid redeclaring 'window' in nested scopes by renaming local variables.
        // - CA1826: Do not use LINQ's FirstOrDefault() on indexable collections; use direct indexing if possible.

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

        // This generates a PickPhotoCommand that the ProfileEditPage can bind to.
        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a profile photo"
                });

                if (result == null)
                    return; // user canceled

                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                var filePath = Path.Combine(FileSystem.AppDataDirectory, "BoatPicture.png");
                var directory = Path.GetDirectoryName(filePath);
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
                if (ProfileItems != null && ProfileItems.Count > 0 && ProfileItems[0] != null)
                {
                    // set to the local file path (MAUI Image accepts local file path strings)
                    ProfileItems[0].ImageSource = filePath;

                    //Save the changed picture to the profile JSON                    
                    await _profileJSONTools.SaveProfileAsync(ProfileItems[0]);
                    await LoadProfileItemsAsync();
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

        // Pseudocode / Plan:
        // 1. Retrieve the profile item from _profileJSONTools.GetProfileAsync().
        // 2. If null -> set ProfileItems to empty collection.
        // 3. If not null -> ensure the profile's ImageSource is a path MAUI Image can use:
        //    a. If ImageSource is empty/null, check for a default file in FileSystem.AppDataDirectory ("BoatPicture.png") and use it if present.
        //    b. If ImageSource starts with "data:" (data URI) or appears to be base64, decode it to bytes and write to AppDataDirectory/"BoatPicture.png", then set ImageSource to that file path.
        //    c. If ImageSource is an absolute URI, leave it as-is (Image can load remote URIs).
        //    d. If ImageSource is a relative/local path, try to resolve it to an absolute path (check as-is, then check AppDataDirectory for filename).
        // 4. Populate ProfileItems with the updated profile item so bound UI picks up the image.
        // 5. Handle failures conservatively (don't throw; fall back to empty collection or leave original ImageSource).

        [RelayCommand]
        public async Task LoadProfileItemsAsync()
        {
            var profileItem = await _profileJSONTools.GetProfileAsync();
            if (profileItem == null)
            {
                ProfileItems = new ObservableCollection<ProfileItem>();

                
            }

         

            try
            {
                // If no image specified, use existing saved file if available
                if (string.IsNullOrWhiteSpace(profileItem.ImageSource))
                {
                    var defaultPath = Path.Combine(FileSystem.AppDataDirectory, "BoatPicture.png");
                    if (File.Exists(defaultPath))
                    {
                        profileItem.ImageSource = defaultPath;
                    }
                }
                else
                {
                    var imageValue = profileItem.ImageSource.Trim();

                    // If data URI (e.g., "data:image/png;base64,...."), strip the prefix
                    if (imageValue.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var commaIndex = imageValue.IndexOf(',');
                        if (commaIndex >= 0)
                        {
                            imageValue = imageValue[(commaIndex + 1)..];
                        }
                    }

                    // Try interpret as base64
                    bool isBase64 = false;
                    byte[]? imageBytes = null;
                    try
                    {
                        imageBytes = Convert.FromBase64String(imageValue);
                        isBase64 = imageBytes != null && imageBytes.Length > 0;
                    }
                    catch
                    {
                        isBase64 = false;
                    }

                    if (isBase64 && imageBytes is not null)
                    {
                        // Write the image bytes to a local file so MAUI Image can load it by path
                        var filePath = Path.Combine(FileSystem.AppDataDirectory, "BoatPicture.png");
                        var directory = Path.GetDirectoryName(filePath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        await File.WriteAllBytesAsync(filePath, imageBytes);
                        profileItem.ImageSource = filePath;
                    }
                    else
                    {
                        // If it's an absolute URI (http/https or file), leave it
                        if (Uri.IsWellFormedUriString(imageValue, UriKind.Absolute))
                        {
                            profileItem.ImageSource = imageValue;
                        }
                        else
                        {
                            // Treat as a local path: try as-is, then try AppDataDirectory with same filename
                            if (File.Exists(imageValue))
                            {
                                profileItem.ImageSource = Path.GetFullPath(imageValue);
                            }
                            else
                            {
                                var candidate = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(imageValue));
                                if (File.Exists(candidate))
                                {
                                    profileItem.ImageSource = candidate;
                                }
                                else
                                {
                                    // The saved absolute path is stale (e.g. old bundle path whose GUID
                                    // changed between builds). Fall back to just the filename so MAUI
                                    // can resolve it as a bundled resource from Resources/Images.
                                    var fileName = Path.GetFileName(imageValue);
                                    if (!string.IsNullOrWhiteSpace(fileName))
                                    {
                                        profileItem.ImageSource = fileName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // If any error occurs processing the image, fallback to original profileItem.ImageSource or a missing image.
                // Do not crash loading the page.
            }

            ProfileItems = new ObservableCollection<ProfileItem> { profileItem };
        }

        [RelayCommand]
        public async Task SaveData()
        {
            if (ProfileItems != null && ProfileItems.Count > 0)
            {
                try
                {
                    await _profileJSONTools.SaveProfileAsync(ProfileItems[0]);

                    // Optional: refresh the view model so the UI reflects persisted state
                    await LoadProfileItemsAsync();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SaveData error: {ex}");
                    await ShowAlertAsync("Error", $"Failed to save profile: {ex.Message}", "OK");
                }
            }
            else
            {
                // Nothing to save: inform the user via the shared alert helper.
                await ShowAlertAsync("Save", "Nothing to save.", "OK");
            }

            var mainWindow2 = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var navigation2 = mainWindow2?.Page?.Navigation;
            if (navigation2 != null)
            {
                await navigation2.PopAsync();
            }
        }

        [RelayCommand]
        public async Task OpenEditPage()
        {
            var mainWindow = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var navigation = mainWindow?.Page?.Navigation;
            if (navigation != null)
            {
                await navigation.PushAsync(new ProfileEditPage());
            }
        }

        [RelayCommand]
        public async Task CalulcateServiceHoursRemaining()
        {
            try
            {
                //Are there any diesel Refill entries in the database?
                databaseItems =
                    await database.GetItemsViaQueryAsync($"Select * from DieselDatabase where LeisureHours != '0' or PropHours != '0' ");

                if (databaseItems.Count == 0)
                {
                    //No entries found - Dont do anything as this is expected when no hours have been logged yet
                    Debug.WriteLine("No diesel hours entries found in database");
                    return;
                }
                else
                {
                    databaseItems.Clear();

                    // Fix: Check for null and count before dereferencing ProfileItems[0]
                    if (ProfileItems == null || ProfileItems.Count == 0)
                    {
                        //Dont do anything as the user may need to create a profile via the Profile Edit Page which can be accesed from the Profile View Page
                        Debug.WriteLine("ProfileItems is null or empty");
                        return;
                    }

                    // Safely convert LastServiceDate to a string formatted "yyyy-MM-dd"
                    string ServiceDate;
                    var lastServiceDate = ProfileItems[0].LastServiceDate;
                    if (lastServiceDate == default(DateTime))
                    {
                        ServiceDate = string.Empty;
                    }
                    else
                    {
                        ServiceDate = lastServiceDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    }

                    databaseItems =
                           await database.GetItemsViaQueryAsync($"SELECT SUM(LeisureHours) AS LeisureHours, SUM(PropHours) AS PropHours FROM DieselDatabase WHERE EntryDate > '{ServiceDate}'");

                    switch (databaseItems.Count)
                    {
                        //Calculate and display percentages
                        case 1:

                            var EngineHoursUsed = databaseItems[0].PropHours + databaseItems[0].LeisureHours;
                            var ServiceIntervalHours = ProfileItems[0].EngineServiceIntervalHours - EngineHoursUsed;

                            if (ServiceIntervalHours < 0)
                            {
                                await ShowAlertAsync("Alert", "Engine service is overdue based on logged hours, edit profile to change last service date!", "OK");
                                return;
                            }

                            if (ProfileItems[0].NextEngineServiceAtHours == ServiceIntervalHours)
                            {
                                //No change in service hours remaining - Dont do anything
                                Debug.WriteLine("No change in service hours remaining");
                                return;

                            }

                            ProfileItems[0].NextEngineServiceAtHours = ServiceIntervalHours;

                           
                            await _profileJSONTools.SaveProfileAsync(ProfileItems[0]);

                            // Optional: refresh the view model so the UI reflects persisted state
                            await LoadProfileItemsAsync();
                

                            break;
                        //error condition - multiple records found
                        default:
                            Debug.WriteLine("Error: Multiple records found when calculating service hours remaining");
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

        public async Task CheckForJsonFile(string jsonFileName)
        {

            var JSONPath = Path.Combine(FileSystem.AppDataDirectory, jsonFileName);

            // Check whether the JSON file exists; log or handle as needed.
            if (File.Exists(JSONPath))
            {
                Debug.WriteLine($"Profile JSON found at {JSONPath}");
            }
            else
            {
                //Show alert and navigate to edit page
                await ShowAlertAsync("Alert", "No Boat details have been found, please save some data", "OK");
                await OpenEditPage();
                return;
            }
        }
    }
}
