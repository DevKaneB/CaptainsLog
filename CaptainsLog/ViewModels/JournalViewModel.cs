using CaptainsLog.DatabaseClasses.Items;
using CaptainsLog.DatabaseClasses.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptainsLog.ViewModels
{
    public partial class JournalViewModel : BaseViewModel
    {
        private readonly JournalSQLTools _journalSQLTools;

        [ObservableProperty]
        public ObservableCollection<JournalItem>? journalItems = new();

        // Journal Entry Properties
        [ObservableProperty]
        public string journalPicturePath = string.Empty;
        [ObservableProperty]
        public string journalDate = string.Empty;
        [ObservableProperty]
        public string journalTitle = string.Empty;
        [ObservableProperty]
        public string journalContent = string.Empty;

        public JournalViewModel(JournalSQLTools journalJSONTools)
        {
            _journalSQLTools = journalJSONTools;
        }
    }
}
