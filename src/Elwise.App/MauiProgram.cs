using Microsoft.Extensions.Logging;
using Elwise.App.ViewModels;
using Elwise.Core.Prices;
using Elwise.Infrastructure.EnergiDataService;

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
        builder.Services.AddSingleton<IPriceSource, EnergiDataServiceClient>();
        builder.Services.AddTransient<PricesViewModel>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
