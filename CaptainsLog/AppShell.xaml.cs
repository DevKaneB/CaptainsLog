namespace CaptainsLog
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Register routes for pages that may be navigated to via Shell routes
            Routing.RegisterRoute(nameof(AddHoursPage), typeof(AddHoursPage));
            Routing.RegisterRoute(nameof(AddDieselLitresPage), typeof(AddDieselLitresPage));
            Routing.RegisterRoute(nameof(JournalPages.JournalEntryPage), typeof(JournalPages.JournalEntryPage));
        }
    }
}
