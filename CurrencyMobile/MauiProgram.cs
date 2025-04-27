using Microsoft.Extensions.Logging;
using CurrencyMobile.Services;
using CurrencyMobile.ViewModels;
using CurrencyMobile.Views;

namespace CurrencyMobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register the WCF client wrapper as a singleton
		builder.Services.AddSingleton<ICurrencyServiceClient, CurrencyServiceClient>();

		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<BalanceViewModel>();
		builder.Services.AddTransient<RatesViewModel>();
		builder.Services.AddTransient<TradeViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();

		// Register Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<BalancePage>();
		builder.Services.AddTransient<RatesPage>();
		builder.Services.AddTransient<TradePage>();
		builder.Services.AddTransient<HistoryPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
