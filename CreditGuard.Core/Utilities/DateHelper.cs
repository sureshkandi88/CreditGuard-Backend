using System;

namespace CreditGuard.Core.Utilities;

public static class DateHelper
{
    public static long GetCurrentUnixTimeSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static DateTime GetDateFromUnixTime(long unixTimeSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
    }

    /// <summary>
    /// Adds a specific number of weekdays (skipping Sundays).
    /// </summary>
    public static DateTime AddWeekdaysExcludingSunday(DateTime startDate, int daysToAdd)
    {
        DateTime result = startDate.Date;
        int added = 0;
        
        while (added < daysToAdd)
        {
            result = result.AddDays(1);
            if (result.DayOfWeek != DayOfWeek.Sunday)
            {
                added++;
            }
        }
        
        return result;
    }

    public static long AddWeekdaysExcludingSunday(long startUnixTimeSeconds, int daysToAdd)
    {
        var start = GetDateFromUnixTime(startUnixTimeSeconds);
        var end = AddWeekdaysExcludingSunday(start, daysToAdd);
        return ((DateTimeOffset)end).ToUnixTimeSeconds();
    }
    
    public static bool IsSameDay(long unixTime1, long unixTime2)
    {
        return GetDateFromUnixTime(unixTime1).Date == GetDateFromUnixTime(unixTime2).Date;
    }
}
