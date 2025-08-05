namespace Salubrity.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTime? ToUtcSafe(this DateTime? dt)
    {
        if (dt == null) return null;

        return dt.Value.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc)
        };
    }
}