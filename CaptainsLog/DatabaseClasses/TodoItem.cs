using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses
{
    public class DieselDatabase
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int LeisureHours { get; set; }
        public int PropHours { get; set; }
        public int DieselRefill { get; set; }
        public int ServiceReset { get; set; }
        public string Date { get; set; }
    }
}
