using Microsoft.Extensions.Logging;
using Elwise.App.ViewModels;
using Elwise.Core.Prices;
using Elwise.Infrastructure.ElwiseApi;
using Elwise.Infrastructure.EnergiDataService;
using Elwise.App.Settings;
using Plugin.LocalNotification;

namespace Elwise.App;

public static class MauiProgram
{
    // The address of our own server, Elwise.Api.
    // It is a dev tunnel to the server on my PC, so phones can reach it over HTTPS.
    private const string ElwiseApiAddress = "https://zpzn5lh4-7140.eun1.devtunnels.ms/";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("SchibstedGrotesk-Regular.ttf", "TextRegular");
                fonts.AddFont("SchibstedGrotesk-Bold.ttf", "TextBold");
            });

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<EnergiDataServiceClient>();
        builder.Services.AddSingleton(new ElwiseApiClient(CreateElwiseApiHttpClient()));

        // Prices come from our server. If the server cannot be reached,
        // the app gets them directly from Energi Data Service, like before.
        builder.Services.AddSingleton<IPriceSource>(services => new CachedPriceSource(
            new FallbackPriceSource(
                services.GetRequiredService<ElwiseApiClient>(),
                services.GetRequiredService<EnergiDataServiceClient>()),
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

    private static HttpClient CreateElwiseApiHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(ElwiseApiAddress),
            Timeout = TimeSpan.FromSeconds(10)
        };

        // A dev tunnel can show a warning page to browsers. The app only wants the JSON.
        httpClient.DefaultRequestHeaders.Add("X-Tunnel-Skip-AntiPhishing-Page", "true");

        return httpClient;
    }
}