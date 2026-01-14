using CaptainsLog.ViewModels;

namespace CaptainsLog.JournalPages;

public partial class JournalEntryPage : ContentPage
{

    private readonly JournalViewModel _journalViewModel;

    public JournalEntryPage()
	{
		InitializeComponent();

        BindingContext = _journalViewModel = new JournalViewModel(new DatabaseClasses.Services.JournalSQLTools());
    }
}