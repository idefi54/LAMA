using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Extensions
{
	public static class DateTimeExtension
	{
		public static DateTime UnixTimeStampMillisecondsToDateTime(long unixTimeStamp)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
			return dateTime;
		}

		public static long ToUnixTimeMilliseconds(this DateTime dateTime)
		{
			long unixTime = ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
			return unixTime;
		}
	}
}
