using Microsoft.Extensions.Logging;
using Elwise.App.ViewModels;
using Elwise.Core.Prices;
using Elwise.Infrastructure.EnergiDataService;
using Elwise.App.Settings;

namespace Elwise.App;

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

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<EnergiDataServiceClient>();
        builder.Services.AddSingleton<IPriceSource>(services => new CachedPriceSource(
            services.GetRequiredService<EnergiDataServiceClient>(),
            TimeProvider.System));
        builder.Services.AddTransient<PricesViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton<AppSettings>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
