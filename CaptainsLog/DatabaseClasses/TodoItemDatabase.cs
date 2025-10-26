using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses
{
    public class TodoItemDatabase
    {
        SQLiteAsyncConnection database;

        //Initialise Database
        async Task Init()
        {
            if (database is not null)
                return;
            database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await database.CreateTableAsync<TodoItem>();
        }
        //Read Database
        public async Task<List<TodoItem>> GetItemsAsync()
        {
            await Init();
            return await database.Table<TodoItem>().ToListAsync();
        }
        //Read Database with sql query
        public async Task<List<TodoItem>> GetItemsViaQueryAsync(string SQLQuery)
        {
            await Init();
            return await database.QueryAsync<TodoItem>(SQLQuery);
        }
        //Read database with ID number
        public async Task<TodoItem> GetItemAsync(int id)
        {
            await Init();
            return await database.Table<TodoItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }
        //Save entry, specified ID for update, 0 ID for insert
        public async Task<int> SaveItemAsync(TodoItem item)
        {
            await Init();
            if (item.ID != 0)
                return await database.UpdateAsync(item);
            else
                return await database.InsertAsync(item);
        }
        //Delete Item
        public async Task<int> DeleteItemAsync(TodoItem item)
        {
            await Init();
            return await database.DeleteAsync(item);
        }



    }
}
