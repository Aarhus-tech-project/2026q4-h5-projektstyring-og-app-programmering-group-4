using System.Text.Json.Serialization;

namespace Elwise.Infrastructure.EnergiDataService;

/// <summary>
/// The JSON response from the DayAheadPrices dataset.
/// </summary>
public sealed record DayAheadPricesResponse(
    [property: JsonPropertyName("records")] IReadOnlyList<DayAheadPriceRecord> Records);