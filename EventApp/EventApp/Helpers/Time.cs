﻿

using System;
using System.Diagnostics;
namespace EventApp
{

    public static class Time
    {

        public static string GetRelativeTime(string timeStampString)
        {

            DateTime thisTime = DateTime.Now;
            bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(thisTime);
            string currentTimeZone = TimeZone.CurrentTimeZone.StandardName;
            TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
            var timeStampDatetime = DateTime.ParseExact(timeStampString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime timeStamp = TimeZoneInfo.ConvertTimeFromUtc(timeStampDatetime, localTimeZone);
            if (isDaylight)
                timeStamp = timeStamp.AddHours(1);

            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            DateTime currentDate = DateTime.Now;
            var ts = new TimeSpan(currentDate.Ticks - timeStamp.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return "Just Now";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }

        }

    }

}
