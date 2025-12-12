using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace CaptainsLog.DatabaseClasses
{
    public class ExpensesItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string? ExpenseType { get; set; }
        public decimal Amount { get; set; }
        // This is stored as a string because SQLite does not have a native DateTime type
        public string? ExpenseDate { get; set; }
        public string? ExpenseDesc { get; set; }
    }
}
