namespace CaptainsLog.BoatExpensesPages;

public class ExpensesTitlePage : ContentPage
{
	public ExpensesTitlePage()
	{
		Content = new VerticalStackLayout
		{
			Children = {
				new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
				}
			}
		};
	}
}