using CaptainsLog.ViewModels;

namespace CaptainsLog.JournalPages;

public partial class JournalEntryPage : ContentPage
{
    public JournalEntryPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is JournalViewModel vm)
        {
            await vm.LoadEntryPageData();
        }
    }
}