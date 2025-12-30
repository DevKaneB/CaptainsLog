using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.DatabaseClasses.Items
{
    public class JournalItem : INotifyPropertyChanged
    {
        private string imageSource;
        public string ImageSource
        {
            get => imageSource;
            set
            {
                if (imageSource == value) return;
                imageSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
            }
        }

        public DateTime EntryDate { get; set; }
        public string Title { get; set; }
        public string Weather { get; set; }
        public string Content { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
