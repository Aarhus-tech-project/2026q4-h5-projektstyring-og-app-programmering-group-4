using System.Globalization;
using System.Net.Http.Json;
using Elwise.Core.Prices;

namespace Elwise.Infrastructure.ElwiseApi;

/// <summary>
/// Gets the prices from our own server, Elwise.Api.
/// </summary>
public sealed class ElwiseApiClient : IPriceSource
{
    private readonly HttpClient _httpClient;

    public ElwiseApiClient(HttpClient httpClient)
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
        string day = firstDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string url = $"api/prices?area={area}&firstDay={day}&dayCount={dayCount}";

        List<PricePoint>? prices =
            await _httpClient.GetFromJsonAsync<List<PricePoint>>(url, cancellationToken);

        if (prices is null)
        {
            throw new InvalidOperationException("Elwise.Api returned no data.");
        }
        return prices;
    }
}