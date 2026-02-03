using CaptainsLog.ViewModels;

namespace CaptainsLog.JournalPages;

public partial class JournalPage : ContentPage
{

	private readonly JournalViewModel _journalViewModel;

    public JournalPage()
	{
		InitializeComponent();

		BindingContext = _journalViewModel = new JournalViewModel(new DatabaseClasses.Services.JournalSQLTools());
    }

}