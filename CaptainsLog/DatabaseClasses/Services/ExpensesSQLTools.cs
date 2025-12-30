using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaptainsLog.DatabaseClasses.Items;
using SQLite;

namespace CaptainsLog.DatabaseClasses.Services
{
    public class ExpensesSQLTools
    {
        SQLiteAsyncConnection database;

        //Initialise Database
        async Task Init()
        {
            if (database is not null)
                return;
            database = new SQLiteAsyncConnection(Constants.SQLDatabasePath, Constants.Flags);
            var result = await database.CreateTableAsync<ExpensesItem>();
        }

        //Read Database
        public async Task<List<ExpensesItem>> GetItemsAsync()
        {
            await Init();
            return await database.Table<ExpensesItem>().ToListAsync();
        }

        //Read Database with sql query
        public async Task<List<ExpensesItem>> GetItemsViaQueryAsync(string SQLQuery)
        {
            await Init();
            return await database.QueryAsync<ExpensesItem>(SQLQuery);
        }

        //Read database with ID number
        public async Task<ExpensesItem> GetItemAsync(int id)
        {
            await Init();
            return await database.Table<ExpensesItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        //Save to database
        public async Task<int> SaveItemAsync(ExpensesItem item)
        {
            await Init();
            if (item.ID != 0)
                return await database.UpdateAsync(item);
            else
                return await database.InsertAsync(item);
        }

        //Delete Item
        public async Task<int> DeleteItemAsync(ExpensesItem item)
        {
            await Init();
            return await database.DeleteAsync(item);
        }

    }
}
