using CaptainsLog.DatabaseClasses;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

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

        [ObservableProperty]
        private ImageSource? boatImageSource;


        private async Task ShowAlertAsync(string title, string message, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }

        [RelayCommand]
        public async Task LoadProfileItemsAsync()
        {
            var profileItem = await _profileJSONTools.GetProfileAsync();
            if (profileItem != null)
            {
                ProfileItems = new ObservableCollection<ProfileItem> { profileItem };
            }
            else
            {
                ProfileItems = new ObservableCollection<ProfileItem>();
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
                // copy to memory so the stream stays usable by ImageSource lambda
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                var filePath = Path.Combine(FileSystem.AppDataDirectory, "BoatPicture.png");
                // Ensure directory exists (AppDataDirectory should exist, but keep safe)
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

                boatImageSource = ImageSource.FromStream(() =>
                {
                    ms.Position = 0;
                    return ms;
                });


            }
            catch (PermissionException)
            {
                // permission denied - inform user or request permissions
                await ShowAlertAsync("Permissions", "Permission to access photos was denied.", "OK");
            }
            catch (Exception ex)
            {
                // general failure
                await ShowAlertAsync("Error", $"Unable to pick photo: {ex.Message}", "OK");
            }
        }
    }
}
