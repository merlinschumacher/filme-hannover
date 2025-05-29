namespace backend.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Rounds a DateTime to the nearest specified TimeSpan.
    /// </summary>
    /// <param name="dt">The DateTime to round.</param>
    /// <param name="d">The TimeSpan to round to.</param>
    /// <returns>A new DateTime rounded to the nearest specified TimeSpan.</returns>
    public static DateTime RoundTo(this DateTime dt, TimeSpan d)
    {
        return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
    }
}