namespace Elwise.Core.Prices;

/// <summary>
/// Remembers fetched prices for a while, so the same days are not fetched on every call.
/// </summary>
/// <remarks>
/// A result is kept for 10 minutes and then fetched again,
/// so tomorrow's prices show up soon after they are published.
/// </remarks>
public sealed class CachedPriceSource : IPriceSource
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private readonly IPriceSource _inner;
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<(PriceArea Area, DateOnly FirstDay, int DayCount), CacheEntry> _cache = new();

    public CachedPriceSource(IPriceSource inner, TimeProvider timeProvider)
    {
        _inner = inner;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<PricePoint>> GetPricesAsync(
        PriceArea area,
        DateOnly firstDay,
        int dayCount,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        var key = (area, firstDay, dayCount);

        if (_cache.TryGetValue(key, out CacheEntry? entry) && entry.IsValid(now))
        {
            return entry.Prices;
        }

        IReadOnlyList<PricePoint> prices = await _inner.GetPricesAsync(area, firstDay, dayCount, cancellationToken);
        _cache[key] = new CacheEntry(prices, now);

        return prices;
    }

    private sealed record CacheEntry(IReadOnlyList<PricePoint> Prices, DateTimeOffset FetchedAt)
    {
        public bool IsValid(DateTimeOffset now) => now - FetchedAt < Lifetime;
    }
}