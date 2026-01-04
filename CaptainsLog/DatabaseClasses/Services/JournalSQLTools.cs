using CaptainsLog.DatabaseClasses.Items;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses.Services
{
    public class JournalSQLTools
    {
        SQLiteAsyncConnection database;

        //Initialise Database
        async Task Init()
        {
            if (database is not null)
                return;
            database = new SQLiteAsyncConnection(Constants.SQLDatabasePath, Constants.Flags);
            var result = await database.CreateTableAsync<JournalItem>();
        }

        //Read Database
        public async Task<List<JournalItem>> GetItemsAsync()
        {
            await Init();
            return await database.Table<JournalItem>().ToListAsync();
        }

        //Read Database with sql query
        public async Task<List<JournalItem>> GetItemsViaQueryAsync(string SQLQuery)
        {
            await Init();
            return await database.QueryAsync<JournalItem>(SQLQuery);
        }

        //Read database with ID number
        public async Task<JournalItem> GetItemAsync(int id)
        {
            await Init();
            return await database.Table<JournalItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveItemAsync(JournalItem item)
        {
            await Init();
            if (item.ID != 0)
                return await database.UpdateAsync(item);
            else
                return await database.InsertAsync(item);
        }

        //Delete Item
        public async Task<int> DeleteItemAsync(JournalItem item)
        {
            await Init();
            return await database.DeleteAsync(item);
        }

    }
}
