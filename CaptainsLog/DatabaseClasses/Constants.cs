using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses
{
    public static class Constants
    {
        //SQL Database Variables
        public static string SQLDatabaseFilename = "DieselDataSQL.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // Open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string SQLDatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, SQLDatabaseFilename);

        //JSON Database Variables
        public static string JSONDatabaseFilename = "ProfilePageDataJSON.json";

        public static string JSONDatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, JSONDatabaseFilename);

        public static string ProfileImageFilename =>
            Path.Combine(FileSystem.AppDataDirectory, "BoatPicture.png");
    }
}
