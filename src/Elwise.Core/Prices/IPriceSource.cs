namespace Elwise.Core.Prices;

/// <summary>
/// Gets electricity prices for a price area.
/// </summary>
public interface IPriceSource
{
    /// <summary>
    /// Gets the 15-minute prices for one or more Danish days in a row, ordered by start time.
    /// </summary>
    Task<IReadOnlyList<PricePoint>> GetPricesAsync(
        PriceArea area,
        DateOnly firstDay,
        int dayCount,
        CancellationToken cancellationToken = default);
}