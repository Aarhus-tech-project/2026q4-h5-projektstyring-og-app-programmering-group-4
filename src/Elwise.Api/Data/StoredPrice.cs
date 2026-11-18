using Elwise.Core.Prices;

namespace Elwise.Api.Data;

/// <summary>
/// One row in the Prices table: the price of one quarter in one price area.
/// </summary>
public sealed class StoredPrice
{
    public int Id { get; set; }

    public PriceArea Area { get; set; }

    /// <summary>
    /// The start of the quarter in UTC.
    /// </summary>
    public DateTime StartUtc { get; set; }

    /// <summary>
    /// The spot price in DKK per kWh.
    /// </summary>
    public decimal PricePerKwh { get; set; }
}