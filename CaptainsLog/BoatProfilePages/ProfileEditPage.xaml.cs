namespace CaptainsLog.BoatProfilePages;

public partial class ProfileEditPage : ContentPage
{
	public ProfileEditPage()
	{
		InitializeComponent();
        LoadProfileImage();
    }

    async void LoadProfileImage()
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, "BoatPicture");
        if (File.Exists(filePath))
        {
            ProfileImage.Source = ImageSource.FromFile(filePath);
        }
    }

    async void OnPickPhotoTapped(object sender, EventArgs e)
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
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            var filePath = Path.Combine(FileSystem.AppDataDirectory, "BoatPicture");
            var fileStream = File.OpenWrite(filePath);
            ms.Position = 0;
            await ms.CopyToAsync(fileStream);

            ProfileImage.Source = ImageSource.FromStream(() =>
            {
                ms.Position = 0;
                return ms;
            });

            //persist the file to local storage
            
        }
        catch (PermissionException)
        {
            // permission denied - inform user or request permissions
            await DisplayAlert("Permissions", "Permission to access photos was denied.", "OK");
        }
        catch (Exception ex)
        {
            // general failure
            await DisplayAlert("Error", $"Unable to pick photo: {ex.Message}", "OK");
        }
    }
}
