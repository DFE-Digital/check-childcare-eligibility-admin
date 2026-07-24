namespace CheckChildcareEligibility.Admin.Models;

public static class DateTimeExtensions
{
    public static TimeZoneInfo TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

    private const string timezone = "GMT Standard Time";

    public static DateTime GetLocalTime(DateTime time)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo);
    }

    public static string ToLocalString12HourFormatReadable(this DateTime datetime)
    {
        return GetLocalTime(datetime).ToString("dd MMM yyyy h:mmtt").Replace("AM", "am").Replace("PM", "pm");
    }
}
