using Elwise.Api.Data;
using Elwise.Core.Prices;
using Elwise.Core.Time;
using Microsoft.EntityFrameworkCore;

namespace Elwise.Api.Prices;
/// <summary>
/// Gives prices from the database. If a day is missing, it first gets the prices
/// from another price source (Energi Data Service) and saves them.
/// </summary>
public sealed class PriceStore
{
    private readonly ElwiseDbContext _db;
    private readonly IPriceSource _source;

    public PriceStore(ElwiseDbContext db, IPriceSource source)
    {
        _db = db;
        _source = source;
    }
    /// <summary>
    /// Gets the 15-minute prices for one or more Danish days in a row, ordered by start time.
    /// </summary>
    public async Task<IReadOnlyList<PricePoint>> GetPricesAsync(
        PriceArea area,
        DateOnly firstDay,
        int dayCount,
        CancellationToken cancellationToken = default)
   
    
    {
        List<PricePoint> saved = await ReadAsync(area, firstDay, dayCount, cancellationToken);

        if (HasEveryDay(saved, firstDay, dayCount))
        {
            return saved;
        }

        IReadOnlyList<PricePoint> fetched =
            await _source.GetPricesAsync(area, firstDay, dayCount, cancellationToken);

        await SaveNewAsync(area, fetched, cancellationToken);

        return await ReadAsync(area, firstDay, dayCount, cancellationToken);
    }
   private async Task<List<PricePoint>> ReadAsync(
        PriceArea area,
        DateOnly firstDay,
        int dayCount,
        CancellationToken cancellationToken)
    {
        // A Danish day starts at 22:00 or 23:00 UTC the day before.
        // So we read one extra day on each side and keep only the Danish days we need.
        DateTime fromUtc = firstDay.AddDays(-1).ToDateTime(TimeOnly.MinValue);
        DateTime toUtc = firstDay.AddDays(dayCount + 1).ToDateTime(TimeOnly.MinValue);
        DateOnly lastDay = firstDay.AddDays(dayCount - 1);

        List<StoredPrice> rows = await _db.Prices
            .Where(price => price.Area == area && price.StartUtc >= fromUtc && price.StartUtc < toUtc)
            .OrderBy(price => price.StartUtc)
            .ToListAsync(cancellationToken);

        
        var prices = new List<PricePoint>();


        foreach (StoredPrice row in rows)
        {
            var start = new DateTimeOffset(DateTime.SpecifyKind(row.StartUtc, DateTimeKind.Utc));
            DateOnly day = DanishDay(start);

            if (day >= firstDay && day <= lastDay)
            {
                prices.Add(new PricePoint(start, row.PricePerKwh));
            }
        }

        return prices;
    }

    private async Task SaveNewAsync(
        PriceArea area,
        IReadOnlyList<PricePoint> fetched,
        CancellationToken cancellationToken)
    {
        if (fetched.Count == 0)
        {
            return;
        }
        DateTime first = fetched.Min(price => price.Start.UtcDateTime);
        DateTime last = fetched.Max(price => price.Start.UtcDateTime);

        List<DateTime> savedStarts = await _db.Prices
            .Where(price => price.Area == area && price.StartUtc >= first && price.StartUtc <= last)
            .Select(price => price.StartUtc)
            .ToListAsync(cancellationToken);

        var known = new HashSet<DateTime>(savedStarts);

        foreach (PricePoint price in fetched)
        {
            DateTime startUtc = price.Start.UtcDateTime;

            // Add returns false when the quarter is already saved.
            if (known.Add(startUtc))
            {
                _db.Prices.Add(new StoredPrice
                {
                    Area = area,
                    StartUtc = startUtc,
                    PricePerKwh = price.PricePerKwh
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static bool HasEveryDay(IReadOnlyList<PricePoint> prices, DateOnly firstDay, int dayCount)
    {
        for (int offset = 0; offset < dayCount; offset++)
        {
            DateOnly day = firstDay.AddDays(offset);

            if (!prices.Any(price => DanishDay(price.Start) == day))
            {
                return false;
            }
        }

        return true;
    }
    private static DateOnly DanishDay(DateTimeOffset time) =>
        DateOnly.FromDateTime(DanishTime.FromUtc(time).DateTime);
}