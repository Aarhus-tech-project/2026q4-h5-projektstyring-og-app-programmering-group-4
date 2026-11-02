namespace Elwise.Core.Prices;

/// <summary>
/// Gets electricity prices for a price area.
/// </summary>
public interface IPriceSource
{
    /// <summary>
    /// Gets the 15-minute prices for one Danish day, ordered by start time.
    /// </summary>
    Task<IReadOnlyList<PricePoint>> GetPricesAsync(
        PriceArea area,
        DateOnly day,
        CancellationToken cancellationToken = default);
}