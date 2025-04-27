namespace CurrencyMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register routes for navigation
		Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
		Routing.RegisterRoute(nameof(Views.BalancePage), typeof(Views.BalancePage));
		Routing.RegisterRoute(nameof(Views.RatesPage), typeof(Views.RatesPage));
		Routing.RegisterRoute(nameof(Views.TradePage), typeof(Views.TradePage));
		Routing.RegisterRoute(nameof(Views.HistoryPage), typeof(Views.HistoryPage));
	}
}
