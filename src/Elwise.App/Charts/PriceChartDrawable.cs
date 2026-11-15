using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using Elwise.Core.Time;
using Microsoft.Maui.Graphics;
using HorizontalAlignment = Microsoft.Maui.Graphics.HorizontalAlignment;
using VerticalAlignment = Microsoft.Maui.Graphics.VerticalAlignment;

namespace Elwise.App.Charts;

/// <summary>
/// Draws the 15-minute prices as thin bars. The cheapest window is marked in green,
/// the hours 17–21 are pale, past prices are faded and a line marks the current time.
/// </summary>
public sealed class PriceChartDrawable : IDrawable
{
    private const float TopSpace = 20f;
    private const float BottomSpace = 18f;

    private static readonly Color BarColor = Color.FromArgb("#A3A3A3");
    private static readonly Color WindowBarColor = Color.FromArgb("#000000");
    private static readonly Color HighlightColor = Color.FromArgb("#7BE0A8");
    private static readonly Color NowColor = Color.FromArgb("#737373");
    private static readonly Color LabelColor = Color.FromArgb("#8A8A8A");
    private static readonly Color DividerColor = Color.FromArgb("#D4D4D4");

    /// <summary>
    /// The prices to draw, ordered by start time.
    /// </summary>
    public IReadOnlyList<PricePoint> Prices { get; set; } = [];

    /// <summary>
    /// The cheapest window, or null if there is none.
    /// </summary>
    public PriceWindow? Cheapest { get; set; }

    /// <summary>
    /// The current time in UTC.
    /// </summary>
    public DateTimeOffset Now { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Prices.Count == 0)
        {
            return;
        }

        float top = dirtyRect.Top + TopSpace;
        float bottom = dirtyRect.Bottom - BottomSpace;
        float height = bottom - top;
        float slot = dirtyRect.Width / Prices.Count;
        float barWidth = Math.Max(slot * 0.7f, 1f);

        float maxPrice = (float)Math.Max(Prices.Max(price => price.PricePerKwh), 0m);
        float minPrice = (float)Math.Min(Prices.Min(price => price.PricePerKwh), 0m);
        float range = maxPrice - minPrice > 0f ? maxPrice - minPrice : 1f;
        float zeroY = bottom - (0f - minPrice) / range * height;

        DrawWindowBand(canvas, dirtyRect, slot, top, bottom);

        canvas.FontColor = LabelColor;
        canvas.FontSize = 10;

        for (int index = 0; index < Prices.Count; index++)
        {
            PricePoint price = Prices[index];
            DateTimeOffset danish = DanishTime.FromUtc(price.Start);
            float x = dirtyRect.Left + index * slot;
            float priceY = bottom - ((float)price.PricePerKwh - minPrice) / range * height;

            canvas.FillColor = ColorFor(price, danish.Hour);
            canvas.FillRectangle(x, Math.Min(priceY, zeroY), barWidth, Math.Max(Math.Abs(zeroY - priceY), 1f));

            if (danish.Minute != 0)
            {
                continue;
            }

            if (danish.Hour == 0)
            {
                DrawDayStart(canvas, dirtyRect, x, bottom, index == 0 ? "I dag" : "I morgen", index > 0);
            }

            if (danish.Hour % 6 == 0)
            {
                canvas.DrawString(
                    danish.Hour.ToString("00"),
                    x, bottom + 4, 30, BottomSpace - 4,
                    HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }

        DrawNowLine(canvas, dirtyRect, slot, bottom);
    }

    private void DrawWindowBand(ICanvas canvas, RectF dirtyRect, float slot, float top, float bottom)
    {
        if (Cheapest is null)
        {
            return;
        }

        float startX = dirtyRect.Left + (float)((Cheapest.Start - Prices[0].Start).TotalMinutes / 15) * slot;
        float endX = dirtyRect.Left + (float)((Cheapest.End - Prices[0].Start).TotalMinutes / 15) * slot;

        canvas.FillColor = HighlightColor;
        canvas.FillRectangle(startX - slot * 0.15f, top - 4, endX - startX, bottom - top + 4);
    }

    private Color ColorFor(PricePoint price, int danishHour)
    {
        bool isInWindow = Cheapest is not null && price.Start >= Cheapest.Start && price.Start < Cheapest.End;

        if (isInWindow)
        {
            return WindowBarColor;
        }

        bool isPeak = danishHour >= CheapestWindowFinder.PeakStartHour && danishHour < CheapestWindowFinder.PeakEndHour;
        Color color = isPeak ? BarColor.WithAlpha(0.3f) : BarColor;

        return price.Start.AddMinutes(15) <= Now ? color.WithAlpha(color.Alpha * 0.5f) : color;
    }
    private static void DrawDayStart(ICanvas canvas, RectF dirtyRect, float x, float bottom, string title, bool drawDivider)
     {
        if (drawDivider)
        {
            canvas.SaveState();
            canvas.StrokeColor = DividerColor;
            canvas.StrokeSize = 1;
            canvas.StrokeDashPattern = [3f, 3f];
            canvas.DrawLine(x, dirtyRect.Top, x, bottom);
            canvas.RestoreState();
        }

        canvas.DrawString(
            title,
            x + 4, dirtyRect.Top, 80, TopSpace - 4,
            HorizontalAlignment.Left, VerticalAlignment.Top);
    }


    private void DrawNowLine(ICanvas canvas, RectF dirtyRect, float slot, float bottom)
    {
        double quarters = (Now - Prices[0].Start).TotalMinutes / 15;

        if (quarters < 0 || quarters > Prices.Count)
        {
            return;
        }

        float x = dirtyRect.Left + (float)quarters * slot;
        float lineTop = dirtyRect.Top + TopSpace - 4;

        canvas.StrokeColor = NowColor;
        canvas.StrokeSize = 1.5f;
        canvas.DrawLine(x, lineTop, x, bottom);
        canvas.FillColor = NowColor;
       canvas.FillCircle(x, lineTop, 3f);
   
    }

}