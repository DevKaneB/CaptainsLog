using CaptainsLog.DatabaseClasses;
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

namespace CaptainsLog.ViewModels
{
    public partial class ProfilePageModel : BaseViewModel
    {
        private readonly ProfileJSONTools _profileJSONTools;


        public ProfilePageModel(ProfileJSONTools profileJSONTools)
        {
            _profileJSONTools = profileJSONTools;
        }

        [ObservableProperty]
        public ObservableCollection<ProfileItem>? profileItems = new();


        private async Task ShowAlertAsync(string title, string message, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
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
                return;
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
                                // else leave original value; Image may handle other schemes
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

                    // Show confirmation to the user
                    await ShowAlertAsync("Saved", "Profile saved successfully.", "OK");
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
        }

        [RelayCommand]
        public async Task OpenEditPage()
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            var navigation = window?.Page?.Navigation;
            if (navigation != null)
            {
                await navigation.PushAsync(new ProfileEditPage());
            }
        }
    }
}
