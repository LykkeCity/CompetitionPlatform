using System;
using System.Globalization;

namespace Common
{
    public static class DateTimeUtils
    {
        public static string ToIsoDate(this DateTime dateTime)
        {
            return dateTime.Year + "-" + dateTime.Month.ToString("00", CultureInfo.InvariantCulture) + "-" +
                   dateTime.Day.ToString("00", CultureInfo.InvariantCulture);
        }


        public static string ToIsoDateTime(this DateTime dateTime)
        {
            return dateTime.ToString(Utils.IsoDateTimeMask);
        }

    }


}