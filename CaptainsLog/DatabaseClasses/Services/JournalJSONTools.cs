using CaptainsLog.DatabaseClasses.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses.Services
{
    public class JournalJSONTools
    {
        private List<JournalItem> journalItems = new();

        async Task init()
        {
            if (!File.Exists(Constants.JournalJSONDatabasePath))
            {
                journalItems = new List<JournalItem>();
                return;
            }

            try
            {
                string json = await File.ReadAllTextAsync(Constants.JournalJSONDatabasePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    journalItems = new List<JournalItem>();
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                var deserialized = JsonSerializer.Deserialize<List<JournalItem>>(json, options);
                journalItems = deserialized ?? new List<JournalItem>();
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading JSON file: {ex.Message}");
                journalItems = new List<JournalItem>();
                return;
            }
        }

        public async Task<List<JournalItem>> GetJournalItemsAsync()
        {
            await init();
            return journalItems;
        }

        public async Task SaveJournalItemsAsync(List<JournalItem> items)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(items, options);
                await File.WriteAllTextAsync(Constants.JournalJSONDatabasePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save journal JSON file: {ex.Message}");
            }
        }
    }
}
