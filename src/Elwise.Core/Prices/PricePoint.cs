namespace Elwise.Core.Prices;

/// <summary>
/// Represents a 15-minute electricity spot price.
/// </summary>
/// <param name="Start">
/// The start time of the 15-minute interval in UTC.
/// </param>
/// <param name="PricePerKwh">
/// The spot price in DKK per kWh, excluding network tariffs, taxes, and VAT.
/// </param>
public sealed record PricePoint(DateTimeOffset Start, decimal PricePerKwh);