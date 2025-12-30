using CaptainsLog.DatabaseClasses.Items;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace CaptainsLog.DatabaseClasses.Services
{
    public class ProfileJSONTools
    {
        private ProfileItem? profileItem;

        async Task init()
        {
            if (!File.Exists(Constants.ProfileJSONDatabasePath))
            {
                profileItem = new ProfileItem();
                return;
            }

            try
            {
                string json = await File.ReadAllTextAsync(Constants.ProfileJSONDatabasePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                profileItem = JsonSerializer.Deserialize<ProfileItem>(json, options);
                return;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to load or parse the profile JSON file.");
                return;
            }
        }

        public async Task<ProfileItem?> GetProfileAsync()
        {
            await init();
            return profileItem;
        }

        public async Task SaveProfileAsync(ProfileItem item)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(item, options);
                await File.WriteAllTextAsync(Constants.ProfileJSONDatabasePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save profile JSON file: {ex.Message}");
            }
        }

    }
}
