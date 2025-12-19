using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.BoatExpensesPages
{
    public class ExpenseResult
    {

        public int ExpenseID { get; set; }
        public string ExpenseDesc { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;

    }
}
