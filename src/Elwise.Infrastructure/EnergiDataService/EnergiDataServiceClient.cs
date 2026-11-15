using System.Globalization;
using System.Net.Http.Json;
using Elwise.Core.Prices;

namespace Elwise.Infrastructure.EnergiDataService;

/// <summary>
/// Gets day-ahead prices from Energi Data Service.
/// </summary>
public sealed class EnergiDataServiceClient : IPriceSource
{
    private const string DatasetUrl = "https://api.energidataservice.dk/dataset/DayAheadPrices";

    private readonly HttpClient _httpClient;

    public EnergiDataServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PricePoint>> GetPricesAsync(
        PriceArea area,
        DateOnly firstDay,
        int dayCount,
        CancellationToken cancellationToken = default)
    {
        string url = BuildUrl(area, firstDay, dayCount);

        DayAheadPricesResponse? response =
            await _httpClient.GetFromJsonAsync<DayAheadPricesResponse>(url, cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException("Energi Data Service returned no data.");
        }

        var prices = new List<PricePoint>();

        foreach (DayAheadPriceRecord record in response.Records)
        {
            prices.Add(record.ToPricePoint());
        }

        return prices;
    }

    private static string BuildUrl(PriceArea area, DateOnly firstDay, int dayCount)
    {
        string start = firstDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string end = firstDay.AddDays(dayCount).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string filter = Uri.EscapeDataString("{\"PriceArea\":[\"" + area + "\"]}");

        return $"{DatasetUrl}?start={start}&end={end}&filter={filter}&sort=TimeUTC&limit=0";
    }
}