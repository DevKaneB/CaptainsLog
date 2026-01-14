using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses.Items
{
    public class JournalItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string EntryDate { get; set; }
        public string PicturePath { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }   
        public string Content { get; set; }

    }
}
