using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Extensions
{
	public static class DateTimeExtension
	{
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime UnixTimeStampMillisecondsToDateTime(long unixTimeStamp)
		{
			DateTime dateTime = UnixEpoch;
			dateTime = dateTime.AddMilliseconds(unixTimeStamp);
			return dateTime;
		}

		public static long ToUnixTimeMilliseconds(this DateTime dateTime)
		{
			long unixTime = ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
			return unixTime;
		}
	}
}
