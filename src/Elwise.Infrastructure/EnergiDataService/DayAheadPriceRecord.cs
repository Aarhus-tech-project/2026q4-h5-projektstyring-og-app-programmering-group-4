using System.Text.Json.Serialization;
using Elwise.Core.Prices;

namespace Elwise.Infrastructure.EnergiDataService;

/// <summary>
/// One 15-minute price from the DayAheadPrices dataset in Energi Data Service.
/// </summary>
public sealed record DayAheadPriceRecord(
   [property: JsonPropertyName("TimeUTC")] DateTime TimeUtc,
   [property: JsonPropertyName("DayAheadPriceDKK")] decimal PriceDkkPerMwh)

{
    /// <summary>
    /// Converts e record to a price point in DKK per kWh.
    /// </summary>  
    public PricePoint ToPricePoint() =>
         new(
             Start: new DateTimeOffset(TimeUtc, TimeSpan.Zero),
             PricePerKwh: PriceDkkPerMwh / 1000m);
}