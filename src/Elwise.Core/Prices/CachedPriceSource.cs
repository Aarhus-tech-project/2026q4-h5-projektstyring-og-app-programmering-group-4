namespace Elwise.Core.Prices;

/// <summary>
/// Remembers fetched prices, so the same day is not fetched again.
/// </summary>
/// <remarks>
/// Published prices do not change, so they are kept while the app runs.
/// An empty result (tomorrow's prices before they are published) is kept for 10 minutes.
/// </remarks>
public sealed class CachedPriceSource : IPriceSource
{
    private static readonly TimeSpan EmptyResultLifetime = TimeSpan.FromMinutes(10);

    private readonly IPriceSource _inner;
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<(PriceArea Area, DateOnly Day), CacheEntry> _cache = new();

    public CachedPriceSource(IPriceSource inner, TimeProvider timeProvider)
    {
        _inner = inner;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<PricePoint>> GetPricesAsync(
        PriceArea area,
        DateOnly day,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();

        if (_cache.TryGetValue((area, day), out CacheEntry? entry) && entry.IsValid(now))
        {
            return entry.Prices;
        }

        IReadOnlyList<PricePoint> prices = await _inner.GetPricesAsync(area, day, cancellationToken);
        _cache[(area, day)] = new CacheEntry(prices, now);

        return prices;
    }

    private sealed record CacheEntry(IReadOnlyList<PricePoint> Prices, DateTimeOffset FetchedAt)
    {
        public bool IsValid(DateTimeOffset now) =>
            Prices.Count > 0 || now - FetchedAt < EmptyResultLifetime;
    }
}