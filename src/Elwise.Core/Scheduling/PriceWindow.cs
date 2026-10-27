namespace Elwise.Core.Scheduling;

/// <summary>
/// Represents a time window with an inclusive start,
/// an exclusive end, and an average electricity spot price.
/// </summary>
/// <param name="Start">
/// The start of the time window in UTC (inclusive).
/// </param>
/// <param name="End">
/// The end of the time window in UTC (exclusive).
/// </param>
/// <param name="AveragePricePerKwh">
/// The average spot price in DKK per kWh,
/// excluding network tariffs, taxes, and VAT.
/// </param>
public sealed record PriceWindow(
    DateTimeOffset Start,
    DateTimeOffset End,
    decimal AveragePricePerKwh
);