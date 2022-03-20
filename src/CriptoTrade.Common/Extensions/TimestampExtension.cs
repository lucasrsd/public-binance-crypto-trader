using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Common.Extensions
{
    public static class TimestampExtension
    {
        public static string GetTimestampNow()
        {
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return milliseconds.ToString();
        }

        public static DateTime FromTimestamp(this long timestamp)
        {
            var date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            return date.DateTime;
        }
    }
}
