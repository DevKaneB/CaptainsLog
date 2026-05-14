using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses.Items
{
    public class DieselDatabase
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int LeisureHours { get; set; }
        public int LeiureMinutes { get; set; }
        public int PropHours { get; set; }
        public int PropMinutes { get; set; }
        public int DieselRefill { get; set; }
        public int ServiceReset { get; set; }
        // This is stored as a string because SQLite does not have a native DateTime type
        public string? EntryDate { get; set; }
    }
}
