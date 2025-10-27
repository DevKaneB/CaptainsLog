using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses
{
    public class TodoItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int DieselHours { get; set; }
        public int PropHours { get; set; }
        public int DieselRefill { get; set; }
        public string Date { get; set; }
    }
}
