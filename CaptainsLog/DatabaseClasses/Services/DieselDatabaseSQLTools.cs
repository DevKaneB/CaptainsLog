using CaptainsLog.DatabaseClasses.Items;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses.Services
{
    public class DieselDatabaseMethods
    {
        SQLiteAsyncConnection database;

        //Initialise Database
        async Task Init()
        {
            if (database is not null)
                return;
            database = new SQLiteAsyncConnection(Constants.SQLDatabasePath, Constants.Flags);
            var result = await database.CreateTableAsync<DieselDatabase>();
        }
        //Read Database
        public async Task<List<DieselDatabase>> GetItemsAsync()
        {
            await Init();
            return await database.Table<DieselDatabase>().ToListAsync();
        }
        //Read Database with sql query
        public async Task<List<DieselDatabase>> GetItemsViaQueryAsync(string SQLQuery)
        {
            await Init();
            return await database.QueryAsync<DieselDatabase>(SQLQuery);
        }
        //Read database with ID number
        public async Task<DieselDatabase> GetItemAsync(int id)
        {
            await Init();
            return await database.Table<DieselDatabase>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }
        //Save entry, specified ID for update, 0 ID for insert
        public async Task<int> SaveItemAsync(DieselDatabase item)
        {
            await Init();
            if (item.ID != 0)
                return await database.UpdateAsync(item);
            else
                return await database.InsertAsync(item);
        }
        //Delete Item
        public async Task<int> DeleteItemAsync(DieselDatabase item)
        {
            await Init();
            return await database.DeleteAsync(item);
        }



    }
}
