namespace Elwise.Core.Prices;

/// <summary>
/// Gets prices from a first price source. If it fails, it uses a fallback price source.
/// </summary>
public sealed class FallbackPriceSource : IPriceSource
{
    private readonly IPriceSource _first;
    private readonly IPriceSource _fallback;

    public FallbackPriceSource(IPriceSource first, IPriceSource fallback)
    {
        _first = first;
        _fallback = fallback;
    }
    /// <inheritdoc />
    public async Task<IReadOnlyList<PricePoint>> GetPricesAsync(
        PriceArea area,
        DateOnly firstDay,
        int dayCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _first.GetPricesAsync(area, firstDay, dayCount, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // The first source failed, for example because the server is offline.
            return await _fallback.GetPricesAsync(area, firstDay, dayCount, cancellationToken);
        }


    }
}