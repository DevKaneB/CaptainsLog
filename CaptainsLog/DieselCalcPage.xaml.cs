namespace CaptainsLog;

public partial class DieselCalcPage : ContentPage
{
    public DieselCalcPage()
    {
        InitializeComponent();
    }

    async void OnPropHrsClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new AddHoursPage());
        }
    }

    async void OnDiesHrsClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new AddHoursPage());
        }
    }

    async void OnLastRefillClicked(object? sender, EventArgs e)
    {

    }

    async void OnLastThirtyDaysClicked(object? sender, EventArgs e)
    {

    }

    async void OnLastYearClicked(object? sender, EventArgs e)
    {

    }

    async void OnDieselRefillClicked(object? sender, EventArgs e)
    {
        var nav = this.Window?.Page?.Navigation;
        if (nav != null)
        {
            await nav.PushAsync(new AddDieselLitresPage());
        }
    }

    async void OnViewHistoryClicked(object? sender, EventArgs e)
    {

    }
}